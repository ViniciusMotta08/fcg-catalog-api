using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Exceptions;
using FCG.Catalog.Application.Mappers;
using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Jogos;
using FCG.Catalog.Domain.Pedidos;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Application.UseCases;

/// <summary>
/// Caso de uso: o usuário pede para adicionar um jogo à biblioteca. Cria o Pedido
/// (aguardando pagamento) e grava o OrderPlacedEvent na Outbox, na mesma transação.
/// </summary>
public sealed class RealizarPedidoUseCase
{
    private readonly IJogoRepository _jogos;
    private readonly IBibliotecaRepository _bibliotecas;
    private readonly IPedidoRepository _pedidos;
    private readonly ServicoDeCompraDeJogos _servicoDeCompra;
    private readonly IUnitOfWork _unitOfWork;

    public RealizarPedidoUseCase(
        IJogoRepository jogos,
        IBibliotecaRepository bibliotecas,
        IPedidoRepository pedidos,
        ServicoDeCompraDeJogos servicoDeCompra,
        IUnitOfWork unitOfWork)
    {
        _jogos = jogos;
        _bibliotecas = bibliotecas;
        _pedidos = pedidos;
        _servicoDeCompra = servicoDeCompra;
        _unitOfWork = unitOfWork;
    }

    public async Task<PedidoResponse> ExecutarAsync(
        Guid usuarioId, string emailDoComprador, RealizarPedidoRequest request, CancellationToken ct = default)
    {
        var jogo = await _jogos.ObterPorIdAsync(request.JogoId, ct)
                   ?? throw new RecursoNaoEncontradoException($"Jogo {request.JogoId} não encontrado.");

        var biblioteca = await _bibliotecas.ObterDoUsuarioAsync(usuarioId, ct);
        var existePedidoEmAndamento = await _pedidos.ExisteAguardandoPagamentoAsync(usuarioId, jogo.Id, ct);

        var pedido = _servicoDeCompra.IniciarCompra(
            usuarioId, emailDoComprador, jogo, biblioteca, existePedidoEmAndamento, DateTime.UtcNow);

        await _pedidos.AdicionarAsync(pedido, ct);
        await _unitOfWork.ConfirmarAsync(ct);

        return CatalogoMapper.ParaResponse(pedido);
    }
}
