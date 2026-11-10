using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Jogos;
using FCG.Catalog.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Persistence.Repositories;

public sealed class JogoRepository : IJogoRepository
{
    private readonly CatalogDbContext _context;

    public JogoRepository(CatalogDbContext context) => _context = context;

    public Task<Jogo?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Jogos.Include(j => j.Promocoes).FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<IReadOnlyList<Jogo>> ListarAsync(int pagina, int tamanhoPagina, bool apenasAtivos, CancellationToken ct = default) =>
        await Filtrar(apenasAtivos)
            .AsNoTracking()
            .Include(j => j.Promocoes)
            .OrderBy(j => j.Titulo)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

    public Task<int> ContarAsync(bool apenasAtivos, CancellationToken ct = default) =>
        Filtrar(apenasAtivos).CountAsync(ct);

    public Task<bool> ExisteComTituloAsync(string titulo, Guid? exceto = null, CancellationToken ct = default) =>
        _context.Jogos.AnyAsync(j => j.Titulo == titulo && (exceto == null || j.Id != exceto), ct);

    public async Task AdicionarAsync(Jogo jogo, CancellationToken ct = default) =>
        await _context.Jogos.AddAsync(jogo, ct);

    private IQueryable<Jogo> Filtrar(bool apenasAtivos) =>
        apenasAtivos ? _context.Jogos.Where(j => j.Ativo) : _context.Jogos;
}

public sealed class PedidoRepository : IPedidoRepository
{
    private readonly CatalogDbContext _context;

    public PedidoRepository(CatalogDbContext context) => _context = context;

    public Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Pedidos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExisteAguardandoPagamentoAsync(Guid usuarioId, Guid jogoId, CancellationToken ct = default) =>
        _context.Pedidos.AnyAsync(p => p.UsuarioId == usuarioId
                                       && p.JogoId == jogoId
                                       && p.Status == StatusDoPedido.AguardandoPagamento, ct);

    public async Task<IReadOnlyList<Pedido>> ListarDoUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await _context.Pedidos
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.RealizadoEm)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Pedido pedido, CancellationToken ct = default) =>
        await _context.Pedidos.AddAsync(pedido, ct);
}

public sealed class BibliotecaRepository : IBibliotecaRepository
{
    private readonly CatalogDbContext _context;

    public BibliotecaRepository(CatalogDbContext context) => _context = context;

    public Task<Biblioteca?> ObterDoUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        _context.Bibliotecas.Include(b => b.Itens).FirstOrDefaultAsync(b => b.Id == usuarioId, ct);

    public async Task AdicionarAsync(Biblioteca biblioteca, CancellationToken ct = default) =>
        await _context.Bibliotecas.AddAsync(biblioteca, ct);
}
