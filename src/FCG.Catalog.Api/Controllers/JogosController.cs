using FCG.Catalog.Api.Extensions;
using FCG.Catalog.Application.Abstractions;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

/// <summary>Catálogo de jogos. Consulta para usuários autenticados; manutenção restrita a administradores.</summary>
[ApiController]
[Route("api/jogos")]
[Authorize]
public sealed class JogosController : ControllerBase
{
    private readonly ConsultarCatalogoUseCase _consultas;
    private readonly GerenciarCatalogoUseCase _gerenciar;

    public JogosController(ConsultarCatalogoUseCase consultas, GerenciarCatalogoUseCase gerenciar)
    {
        _consultas = consultas;
        _gerenciar = gerenciar;
    }

    /// <summary>Lista paginada. Usuários comuns veem apenas jogos ativos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginado<JogoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        [FromQuery] bool incluirInativos = false,
        CancellationToken ct = default)
    {
        var apenasAtivos = !(incluirInativos && User.IsInRole(ClaimsPrincipalExtensions.PerfilAdministrador));
        return Ok(await _consultas.ListarAsync(pagina, tamanhoPagina, apenasAtivos, ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JogoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct) =>
        Ok(await _consultas.ObterPorIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = ClaimsPrincipalExtensions.PerfilAdministrador)]
    [ProducesResponseType(typeof(JogoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarJogoRequest request, CancellationToken ct)
    {
        var jogo = await _gerenciar.CadastrarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = jogo.Id }, jogo);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ClaimsPrincipalExtensions.PerfilAdministrador)]
    [ProducesResponseType(typeof(JogoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarJogoRequest request, CancellationToken ct) =>
        Ok(await _gerenciar.AtualizarAsync(id, request, ct));

    /// <summary>Desativa o jogo (exclusão lógica): deixa de aparecer e não pode ser comprado.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ClaimsPrincipalExtensions.PerfilAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await _gerenciar.DesativarAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/ativar")]
    [Authorize(Roles = ClaimsPrincipalExtensions.PerfilAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Ativar(Guid id, CancellationToken ct)
    {
        await _gerenciar.AtivarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/promocoes")]
    [Authorize(Roles = ClaimsPrincipalExtensions.PerfilAdministrador)]
    [ProducesResponseType(typeof(PromocaoResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CriarPromocao(Guid id, [FromBody] CriarPromocaoRequest request, CancellationToken ct)
    {
        var promocao = await _gerenciar.CriarPromocaoAsync(id, request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id }, promocao);
    }

    [HttpPatch("{id:guid}/promocoes/{promocaoId:guid}/encerrar")]
    [Authorize(Roles = ClaimsPrincipalExtensions.PerfilAdministrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EncerrarPromocao(Guid id, Guid promocaoId, CancellationToken ct)
    {
        await _gerenciar.EncerrarPromocaoAsync(id, promocaoId, ct);
        return NoContent();
    }
}
