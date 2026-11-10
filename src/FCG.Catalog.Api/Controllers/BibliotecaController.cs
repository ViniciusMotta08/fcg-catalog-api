using FCG.Catalog.Api.Extensions;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

/// <summary>Biblioteca e pedidos de compra do usuário autenticado.</summary>
[ApiController]
[Route("api/biblioteca")]
[Authorize]
public sealed class BibliotecaController : ControllerBase
{
    private readonly ConsultarComprasUseCase _consultas;

    public BibliotecaController(ConsultarComprasUseCase consultas) => _consultas = consultas;

    /// <summary>Jogos já adquiridos (pagamento aprovado).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ItemDaBibliotecaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct) =>
        Ok(await _consultas.ListarBibliotecaAsync(User.ObterUsuarioId(), ct));

    /// <summary>
    /// Solicita a inclusão de um jogo na biblioteca: cria o pedido e publica o OrderPlacedEvent.
    /// Retorna 202 — o jogo entra na biblioteca quando o pagamento for aprovado.
    /// </summary>
    [HttpPost("pedidos")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RealizarPedido(
        [FromBody] RealizarPedidoRequest request,
        [FromServices] RealizarPedidoUseCase useCase,
        CancellationToken ct)
    {
        var pedido = await useCase.ExecutarAsync(User.ObterUsuarioId(), User.ObterEmail(), request, ct);
        return AcceptedAtAction(nameof(ObterPedido), new { pedidoId = pedido.Id }, pedido);
    }

    [HttpGet("pedidos")]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPedidos(CancellationToken ct) =>
        Ok(await _consultas.ListarPedidosAsync(User.ObterUsuarioId(), ct));

    /// <summary>Acompanha o status do pedido (AguardandoPagamento, Pago, PagamentoRecusado).</summary>
    [HttpGet("pedidos/{pedidoId:guid}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPedido(Guid pedidoId, CancellationToken ct) =>
        Ok(await _consultas.ObterPedidoAsync(
            User.ObterUsuarioId(), pedidoId, User.IsInRole(ClaimsPrincipalExtensions.PerfilAdministrador), ct));
}
