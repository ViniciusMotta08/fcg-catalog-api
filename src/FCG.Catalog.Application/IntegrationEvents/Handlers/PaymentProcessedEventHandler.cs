using FCG.Catalog.Application.Abstractions;
using FCG.Catalog.Application.UseCases;

namespace FCG.Catalog.Application.IntegrationEvents.Handlers;

/// <summary>Adaptador do evento recebido para o caso de uso de conclusão do pedido.</summary>
public sealed class PaymentProcessedEventHandler : IIntegrationEventHandler<PaymentProcessedEvent>
{
    private readonly ConcluirPedidoUseCase _concluirPedido;

    public PaymentProcessedEventHandler(ConcluirPedidoUseCase concluirPedido) => _concluirPedido = concluirPedido;

    public Task TratarAsync(PaymentProcessedEvent evento, CancellationToken ct = default) =>
        evento.Status == PaymentStatus.Approved
            ? _concluirPedido.ConfirmarPagamentoAsync(evento.OrderId, ct)
            : _concluirPedido.RecusarPagamentoAsync(evento.OrderId, evento.Reason, ct);
}
