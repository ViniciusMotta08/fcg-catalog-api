using FCG.Catalog.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.Catalog.Infrastructure.Messaging.Consumidores;

/// <summary>Consome o resultado do pagamento para concluir o pedido.</summary>
public sealed class PaymentProcessedConsumidor : ConsumidorDeEventoBase<PaymentProcessedEvent>
{
    public PaymentProcessedConsumidor(
        RabbitMqConexao conexao,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentProcessedConsumidor> logger)
        : base(conexao, options, scopeFactory, logger) { }

    protected override string ChaveDaFila => "PaymentProcessed";
    protected override string RoutingKey => PaymentProcessedEvent.Rota;
}
