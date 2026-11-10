using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Application.Abstractions;

/// <summary>
/// Traduz eventos de domínio (linguagem interna) em eventos de integração (linguagem publicada).
/// Um evento de domínio pode não gerar nenhum evento de integração.
/// </summary>
public interface IIntegrationEventMapper
{
    IEnumerable<IntegrationEvent> Mapear(IDomainEvent eventoDeDominio);
}
