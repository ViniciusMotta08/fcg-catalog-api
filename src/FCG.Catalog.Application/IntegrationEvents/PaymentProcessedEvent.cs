using FCG.Catalog.Application.Abstractions;

namespace FCG.Catalog.Application.IntegrationEvents;

public enum PaymentStatus
{
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// Resultado do pagamento publicado pelo PaymentsAPI. Consumido aqui para concluir o pedido
/// e, se aprovado, adicionar o jogo à biblioteca do usuário.
/// </summary>
public sealed record PaymentProcessedEvent : IntegrationEvent
{
    public const string Rota = "payment.processed";

    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public Guid GameId { get; init; }
    public string GameTitle { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public PaymentStatus Status { get; init; }
    public string? Reason { get; init; }
    public DateTime ProcessedAt { get; init; }

    public override string RoutingKey => Rota;
}
