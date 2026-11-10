using FCG.Catalog.Application.Abstractions;
using FCG.Catalog.Application.IntegrationEvents;
using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Jogos;
using FCG.Catalog.Domain.Pedidos;
using FCG.Catalog.Domain.SeedWork;
using FCG.Catalog.Infrastructure.Messaging;
using FCG.Catalog.Infrastructure.Messaging.Consumidores;
using FCG.Catalog.Infrastructure.Messaging.Outbox;
using FCG.Catalog.Infrastructure.Persistence;
using FCG.Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Catalog.Infrastructure;

/// <summary>Registro dos adaptadores de infraestrutura (banco e mensageria).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfraestrutura(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:CatalogDb não configurada.");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(5)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
        services.AddScoped<IJogoRepository, JogoRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IBibliotecaRepository, BibliotecaRepository>();
        services.AddSingleton<IIntegrationEventMapper, MapeadorDeEventosDeIntegracao>();

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.Secao));
        services.AddSingleton<RabbitMqConexao>();
        services.AddSingleton<PublicadorRabbitMq>();
        services.AddHostedService<ProcessadorDeOutbox<CatalogDbContext>>();
        services.AddHostedService<PaymentProcessedConsumidor>();

        return services;
    }
}
