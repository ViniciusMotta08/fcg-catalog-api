using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Pedidos;

/// <summary>Um usuário iniciou a compra de um jogo; o pagamento ainda não foi processado.</summary>
public sealed record PedidoRealizado(
    Guid PedidoId, Guid UsuarioId, string EmailDoComprador, Guid JogoId, string TituloDoJogo, decimal Valor) : DomainEvent;

/// <summary>O pagamento do pedido foi aprovado.</summary>
public sealed record PedidoPago(Guid PedidoId, Guid UsuarioId, Guid JogoId) : DomainEvent;

/// <summary>O pagamento do pedido foi recusado.</summary>
public sealed record PagamentoDoPedidoRecusado(Guid PedidoId, Guid UsuarioId, Guid JogoId, string Motivo) : DomainEvent;
