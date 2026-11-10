using FCG.Catalog.Api.Extensions;
using FCG.Catalog.Api.Middlewares;
using FCG.Catalog.Application.Abstractions;
using FCG.Catalog.Application.IntegrationEvents;
using FCG.Catalog.Application.IntegrationEvents.Handlers;
using FCG.Catalog.Application.UseCases;
using FCG.Catalog.Domain.Pedidos;
using FCG.Catalog.Infrastructure;
using FCG.Catalog.Infrastructure.Messaging;
using FCG.Catalog.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((contexto, configuracao) => configuracao.ReadFrom.Configuration(contexto.Configuration));

// Presentation
builder.Services.AddControllers();
builder.Services.AdicionarSwagger("FCG CatalogAPI", "CRUD de jogos, promoções, biblioteca e início do fluxo de compra.");
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);

// Domain services (sem estado)
builder.Services.AddSingleton<ServicoDeCompraDeJogos>();

// Application (casos de uso e handlers de eventos)
builder.Services.AddScoped<GerenciarCatalogoUseCase>();
builder.Services.AddScoped<ConsultarCatalogoUseCase>();
builder.Services.AddScoped<RealizarPedidoUseCase>();
builder.Services.AddScoped<ConcluirPedidoUseCase>();
builder.Services.AddScoped<ConsultarComprasUseCase>();
builder.Services.AddScoped<IIntegrationEventHandler<PaymentProcessedEvent>, PaymentProcessedEventHandler>();

// Infrastructure
builder.Services.AdicionarInfraestrutura(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CatalogDbContext>("sqlserver", tags: new[] { ApiExtensions.TagProntidao })
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: new[] { ApiExtensions.TagProntidao });

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<TratamentoDeErrosMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapearHealthChecks();

await app.InicializarBancoDeDadosAsync<CatalogDbContext>();

app.Run();

public partial class Program { }
