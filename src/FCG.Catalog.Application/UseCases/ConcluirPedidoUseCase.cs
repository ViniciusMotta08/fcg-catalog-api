using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Pedidos;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Application.UseCases;

/// <summary>
/// Caso de uso disparado pelo PaymentProcessedEvent. Idempotente: reprocessar a mesma
/// mensagem não duplica o jogo na biblioteca nem altera um pedido já concluído.
/// </summary>
/// <remarks>
/// Decisão consciente: Pedido e Biblioteca pertencem ao mesmo bounded context e ao mesmo banco;
/// são confirmados na mesma transação local para nunca existir "pedido pago sem jogo na biblioteca".
/// </remarks>
public sealed class ConcluirPedidoUseCase
{
    private readonly IPedidoRepository _pedidos;
    private readonly IBibliotecaRepository _bibliotecas;
    private readonly IUnitOfWork _unitOfWork;

    public ConcluirPedidoUseCase(IPedidoRepository pedidos, IBibliotecaRepository bibliotecas, IUnitOfWork unitOfWork)
    {
        _pedidos = pedidos;
        _bibliotecas = bibliotecas;
        _unitOfWork = unitOfWork;
    }

    public async Task ConfirmarPagamentoAsync(Guid pedidoId, CancellationToken ct = default)
    {
        var pedido = await ObterPedidoAsync(pedidoId, ct);

        if (!pedido.ConfirmarPagamento())
            return; // Mensagem repetida: pedido já estava pago.

        var biblioteca = await _bibliotecas.ObterDoUsuarioAsync(pedido.UsuarioId, ct);
        if (biblioteca is null)
        {
            biblioteca = Biblioteca.CriarPara(pedido.UsuarioId);
            await _bibliotecas.AdicionarAsync(biblioteca, ct);
        }

        biblioteca.AdicionarJogo(pedido.JogoId, pedido.TituloDoJogo, pedido.Valor, pedido.Id);
        await _unitOfWork.ConfirmarAsync(ct);
    }

    public async Task RecusarPagamentoAsync(Guid pedidoId, string? motivo, CancellationToken ct = default)
    {
        var pedido = await ObterPedidoAsync(pedidoId, ct);

        if (pedido.RecusarPagamento(motivo))
            await _unitOfWork.ConfirmarAsync(ct);
    }

    private async Task<Pedido> ObterPedidoAsync(Guid pedidoId, CancellationToken ct) =>
        await _pedidos.ObterPorIdAsync(pedidoId, ct)
        ?? throw new InvalidOperationException($"Pedido {pedidoId} não encontrado para concluir o pagamento.");
}
