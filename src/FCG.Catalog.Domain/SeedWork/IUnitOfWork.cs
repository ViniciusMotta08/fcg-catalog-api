namespace FCG.Catalog.Domain.SeedWork;

/// <summary>
/// Unidade de trabalho: confirma, em uma única transação, as mudanças dos agregados
/// e os eventos de integração derivados dos seus eventos de domínio (Outbox).
/// </summary>
public interface IUnitOfWork
{
    Task<int> ConfirmarAsync(CancellationToken ct = default);
}
