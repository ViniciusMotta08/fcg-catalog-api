using FCG.Catalog.Application.Abstractions;
using FCG.Catalog.Domain.Pedidos;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Application.IntegrationEvents;

/// <summary>Tradução dos eventos de domínio do Catálogo para a linguagem publicada.</summary>
public sealed class MapeadorDeEventosDeIntegracao : IIntegrationEventMapper
{
    public IEnumerable<IntegrationEvent> Mapear(IDomainEvent eventoDeDominio)
    {
        if (eventoDeDominio is PedidoRealizado pedido)
        {
            yield return new OrderPlacedEvent
            {
                OccurredAt = pedido.OcorreuEm,
                OrderId = pedido.PedidoId,
                UserId = pedido.UsuarioId,
                UserEmail = pedido.EmailDoComprador,
                GameId = pedido.JogoId,
                GameTitle = pedido.TituloDoJogo,
                Price = pedido.Valor
            };
        }

        // PedidoPago, PagamentoDoPedidoRecusado e JogoAdicionadoABiblioteca são eventos internos:
        // não há outro contexto interessado neles.
    }
}
