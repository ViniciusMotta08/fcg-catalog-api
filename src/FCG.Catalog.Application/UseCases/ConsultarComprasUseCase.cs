using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Exceptions;
using FCG.Catalog.Application.Mappers;
using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Pedidos;

namespace FCG.Catalog.Application.UseCases;

/// <summary>Consultas de pedidos e da biblioteca do usuário autenticado.</summary>
public sealed class ConsultarComprasUseCase
{
    private readonly IPedidoRepository _pedidos;
    private readonly IBibliotecaRepository _bibliotecas;

    public ConsultarComprasUseCase(IPedidoRepository pedidos, IBibliotecaRepository bibliotecas)
    {
        _pedidos = pedidos;
        _bibliotecas = bibliotecas;
    }

    public async Task<PedidoResponse> ObterPedidoAsync(Guid usuarioId, Guid pedidoId, bool ehAdministrador, CancellationToken ct = default)
    {
        var pedido = await _pedidos.ObterPorIdAsync(pedidoId, ct);

        // Usuário comum só enxerga os próprios pedidos (404 para não revelar pedidos de terceiros).
        if (pedido is null || (!ehAdministrador && pedido.UsuarioId != usuarioId))
            throw new RecursoNaoEncontradoException($"Pedido {pedidoId} não encontrado.");

        return CatalogoMapper.ParaResponse(pedido);
    }

    public async Task<IReadOnlyList<PedidoResponse>> ListarPedidosAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var pedidos = await _pedidos.ListarDoUsuarioAsync(usuarioId, ct);
        return pedidos.Select(CatalogoMapper.ParaResponse).ToList();
    }

    public async Task<IReadOnlyList<ItemDaBibliotecaResponse>> ListarBibliotecaAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var biblioteca = await _bibliotecas.ObterDoUsuarioAsync(usuarioId, ct);

        return biblioteca?.Itens
                   .OrderByDescending(i => i.AdquiridoEm)
                   .Select(CatalogoMapper.ParaResponse)
                   .ToList()
               ?? new List<ItemDaBibliotecaResponse>();
    }
}
