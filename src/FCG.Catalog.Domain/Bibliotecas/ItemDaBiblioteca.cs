using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Bibliotecas;

/// <summary>Entidade interna do agregado Biblioteca: um jogo adquirido.</summary>
public sealed class ItemDaBiblioteca : Entity<Guid>
{
    private ItemDaBiblioteca() { }

    public Guid JogoId { get; private set; }
    public string TituloDoJogo { get; private set; } = string.Empty;
    public Dinheiro PrecoPago { get; private set; } = null!;
    public Guid PedidoId { get; private set; }
    public DateTime AdquiridoEm { get; private set; }

    internal static ItemDaBiblioteca Criar(Guid jogoId, string tituloDoJogo, Dinheiro precoPago, Guid pedidoId) => new()
    {
        Id = Guid.NewGuid(),
        JogoId = jogoId,
        TituloDoJogo = tituloDoJogo,
        PrecoPago = precoPago,
        PedidoId = pedidoId,
        AdquiridoEm = DateTime.UtcNow
    };
}
