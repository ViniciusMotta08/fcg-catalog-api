namespace FCG.Catalog.Domain.Jogos;

public interface IJogoRepository
{
    /// <summary>Carrega o agregado completo (com promoções).</summary>
    Task<Jogo?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Jogo>> ListarAsync(int pagina, int tamanhoPagina, bool apenasAtivos, CancellationToken ct = default);
    Task<int> ContarAsync(bool apenasAtivos, CancellationToken ct = default);
    Task<bool> ExisteComTituloAsync(string titulo, Guid? exceto = null, CancellationToken ct = default);
    Task AdicionarAsync(Jogo jogo, CancellationToken ct = default);
}
