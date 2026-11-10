using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Jogos;
using FCG.Catalog.Domain.Pedidos;

namespace FCG.Catalog.Application.Mappers;

public static class CatalogoMapper
{
    public static JogoResponse ParaResponse(Jogo jogo) => new(
        jogo.Id,
        jogo.Titulo,
        jogo.Descricao,
        jogo.Genero,
        jogo.Preco.Valor,
        jogo.ObterPrecoVigente(DateTime.UtcNow).Valor,
        jogo.DataLancamento,
        jogo.Ativo,
        jogo.Promocoes.OrderBy(p => p.Vigencia.Inicio).Select(ParaResponse).ToList());

    public static PromocaoResponse ParaResponse(Promocao promocao) => new(
        promocao.Id,
        promocao.Nome,
        promocao.Desconto.Valor,
        promocao.Vigencia.Inicio,
        promocao.Vigencia.Fim,
        promocao.Ativa);

    public static PedidoResponse ParaResponse(Pedido pedido) => new(
        pedido.Id,
        pedido.UsuarioId,
        pedido.JogoId,
        pedido.TituloDoJogo,
        pedido.Valor.Valor,
        pedido.Status.ToString(),
        pedido.MotivoDaRecusa,
        pedido.RealizadoEm,
        pedido.FinalizadoEm);

    public static ItemDaBibliotecaResponse ParaResponse(ItemDaBiblioteca item) => new(
        item.JogoId, item.TituloDoJogo, item.PrecoPago.Valor, item.PedidoId, item.AdquiridoEm);
}
