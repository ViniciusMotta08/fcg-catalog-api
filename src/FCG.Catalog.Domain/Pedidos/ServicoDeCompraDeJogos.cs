using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Jogos;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Pedidos;

/// <summary>
/// Serviço de Domínio: iniciar a compra envolve regras que atravessam três agregados
/// (Jogo, Biblioteca e Pedido) e não pertencem naturalmente a nenhum deles.
/// </summary>
public sealed class ServicoDeCompraDeJogos
{
    public Pedido IniciarCompra(
        Guid usuarioId,
        string emailDoComprador,
        Jogo jogo,
        Biblioteca? bibliotecaDoUsuario,
        bool existePedidoAguardandoPagamento,
        DateTime dataDaCompra)
    {
        if (jogo is null)
            throw new DomainException("Jogo é obrigatório para a compra.");

        if (!jogo.Ativo)
            throw new DomainException("Jogo indisponível para compra.");

        if (bibliotecaDoUsuario?.Possui(jogo.Id) == true)
            throw new DomainException($"O jogo '{jogo.Titulo}' já está na sua biblioteca.");

        if (existePedidoAguardandoPagamento)
            throw new DomainException($"Já existe um pedido do jogo '{jogo.Titulo}' aguardando pagamento.");

        return Pedido.Realizar(usuarioId, emailDoComprador, jogo.Id, jogo.Titulo, jogo.ObterPrecoVigente(dataDaCompra));
    }
}
