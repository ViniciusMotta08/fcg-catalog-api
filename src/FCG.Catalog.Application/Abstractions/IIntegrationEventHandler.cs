namespace FCG.Catalog.Application.Abstractions;

/// <summary>Caso de uso disparado pelo recebimento de um evento de integração.</summary>
public interface IIntegrationEventHandler<in TEvento> where TEvento : IntegrationEvent
{
    Task TratarAsync(TEvento evento, CancellationToken ct = default);
}
