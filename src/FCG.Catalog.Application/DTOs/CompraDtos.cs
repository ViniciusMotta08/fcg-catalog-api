namespace FCG.Catalog.Application.DTOs;

/// <summary>Solicitação para adicionar um jogo à biblioteca (inicia o fluxo de compra).</summary>
public sealed record RealizarPedidoRequest(Guid JogoId);

public sealed record PedidoResponse(
    Guid Id,
    Guid UsuarioId,
    Guid JogoId,
    string TituloDoJogo,
    decimal Valor,
    string Status,
    string? MotivoDaRecusa,
    DateTime RealizadoEm,
    DateTime? FinalizadoEm);

public sealed record ItemDaBibliotecaResponse(Guid JogoId, string TituloDoJogo, decimal PrecoPago, Guid PedidoId, DateTime AdquiridoEm);
