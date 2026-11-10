using FCG.Catalog.Application.Abstractions;

namespace FCG.Catalog.Application.IntegrationEvents;

/// <summary>
/// Publicado quando o usuário solicita a compra de um jogo. Consumido pelo PaymentsAPI.
/// </summary>
public sealed record OrderPlacedEvent : IntegrationEvent
{
    public const string Rota = "order.placed";

    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public Guid GameId { get; init; }
    public string GameTitle { get; init; } = string.Empty;
    public decimal Price { get; init; }

    public override string RoutingKey => Rota;
}
