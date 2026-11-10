using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FCG.Catalog.Api.Extensions;

/// <summary>Configurações transversais da camada de apresentação (HTTP).</summary>
public static class ApiExtensions
{
    public const string TagProntidao = "ready";

    /// <summary>
    /// Validação do JWT emitido pelo UsersAPI. Secret vem do Secret do Kubernetes;
    /// Issuer e Audience vêm do ConfigMap.
    /// </summary>
    public static IServiceCollection AdicionarAutenticacaoJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var secao = configuration.GetSection("Jwt");
        var segredo = secao["Secret"];

        if (string.IsNullOrWhiteSpace(segredo) || segredo.Length < 32)
            throw new InvalidOperationException("Jwt:Secret deve ser informado e ter no mínimo 32 caracteres.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo)),
                    ValidateIssuer = true,
                    ValidIssuer = secao["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = secao["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AdicionarSwagger(this IServiceCollection services, string titulo, string descricao)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = titulo, Version = "v1", Description = descricao });

            var esquema = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe apenas o token JWT emitido pelo UsersAPI (POST /api/auth/login).",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };

            options.AddSecurityDefinition("Bearer", esquema);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquema] = Array.Empty<string>() });

            var xml = Path.Combine(AppContext.BaseDirectory, $"{typeof(ApiExtensions).Assembly.GetName().Name}.xml");
            if (File.Exists(xml))
                options.IncludeXmlComments(xml);
        });

        return services;
    }

    /// <summary>
    /// /health/live: o processo está de pé (livenessProbe).
    /// /health/ready: dependências (banco/broker) acessíveis (readinessProbe).
    /// </summary>
    public static WebApplication MapearHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains(TagProntidao) });
        return app;
    }

    /// <summary>
    /// Cria/atualiza a estrutura do banco SOMENTE se "Banco:CriarEstruturaAutomaticamente" = true.
    /// Usa as migrations do projeto quando existirem; sem migrations, cria o schema a partir do modelo.
    /// Não insere nenhum dado.
    /// </summary>
    public static async Task InicializarBancoDeDadosAsync<TContext>(this WebApplication app) where TContext : DbContext
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Banco");

        if (!app.Configuration.GetValue<bool>("Banco:CriarEstruturaAutomaticamente"))
        {
            logger.LogInformation("Criação automática da estrutura do banco desabilitada (Banco:CriarEstruturaAutomaticamente=false).");
            return;
        }

        const int maximoDeTentativas = 12;

        for (var tentativa = 1; tentativa <= maximoDeTentativas; tentativa++)
        {
            try
            {
                using var escopo = app.Services.CreateScope();
                var context = escopo.ServiceProvider.GetRequiredService<TContext>();

                if (context.Database.GetMigrations().Any())
                    await context.Database.MigrateAsync();
                else
                    await context.Database.EnsureCreatedAsync();

                logger.LogInformation("Estrutura do banco verificada.");
                return;
            }
            catch (Exception ex) when (tentativa < maximoDeTentativas)
            {
                logger.LogWarning(ex, "Banco indisponível (tentativa {Tentativa}/{Maximo}). Aguardando 5s...",
                    tentativa, maximoDeTentativas);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
