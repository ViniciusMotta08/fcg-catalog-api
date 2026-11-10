namespace FCG.Catalog.Domain.Bibliotecas;

public interface IBibliotecaRepository
{
    /// <summary>Carrega o agregado completo (com itens).</summary>
    Task<Biblioteca?> ObterDoUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Biblioteca biblioteca, CancellationToken ct = default);
}
