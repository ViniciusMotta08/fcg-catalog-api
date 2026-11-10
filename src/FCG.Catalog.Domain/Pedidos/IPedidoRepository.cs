namespace FCG.Catalog.Domain.Pedidos;

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteAguardandoPagamentoAsync(Guid usuarioId, Guid jogoId, CancellationToken ct = default);
    Task<IReadOnlyList<Pedido>> ListarDoUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Pedido pedido, CancellationToken ct = default);
}
