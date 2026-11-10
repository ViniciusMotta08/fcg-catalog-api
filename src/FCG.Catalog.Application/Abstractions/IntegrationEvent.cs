using System.Text.Json.Serialization;

namespace FCG.Catalog.Application.Abstractions;

/// <summary>
/// Evento de integração: contrato público (Published Language) trocado entre os
/// bounded contexts via mensageria. Diferente do evento de domínio, é estável e serializável.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    /// <summary>Chave de roteamento no exchange de eventos (não vai no corpo da mensagem).</summary>
    [JsonIgnore]
    public abstract string RoutingKey { get; }
}
