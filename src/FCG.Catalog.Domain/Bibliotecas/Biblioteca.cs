using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Bibliotecas;

/// <summary>Um jogo foi adicionado à biblioteca do usuário.</summary>
public sealed record JogoAdicionadoABiblioteca(Guid UsuarioId, Guid JogoId, Guid PedidoId) : DomainEvent;

/// <summary>
/// Raiz de Agregação da biblioteca de jogos de um usuário. A identidade é o próprio Id do usuário.
/// Invariante: um jogo aparece no máximo uma vez.
/// </summary>
public sealed class Biblioteca : AggregateRoot<Guid>
{
    private readonly List<ItemDaBiblioteca> _itens = new();

    private Biblioteca() { }

    public Guid UsuarioId => Id;
    public DateTime CriadaEm { get; private set; }

    public IReadOnlyCollection<ItemDaBiblioteca> Itens => _itens.AsReadOnly();

    public static Biblioteca CriarPara(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new DomainException("Usuário da biblioteca é obrigatório.");

        return new Biblioteca { Id = usuarioId, CriadaEm = DateTime.UtcNow };
    }

    public bool Possui(Guid jogoId) => _itens.Any(i => i.JogoId == jogoId);

    /// <summary>Adiciona o jogo. Idempotente: retorna false se o jogo já estava na biblioteca.</summary>
    public bool AdicionarJogo(Guid jogoId, string tituloDoJogo, Dinheiro precoPago, Guid pedidoId)
    {
        if (Possui(jogoId))
            return false;

        _itens.Add(ItemDaBiblioteca.Criar(jogoId, tituloDoJogo, precoPago, pedidoId));
        RegistrarEvento(new JogoAdicionadoABiblioteca(Id, jogoId, pedidoId));
        return true;
    }
}
