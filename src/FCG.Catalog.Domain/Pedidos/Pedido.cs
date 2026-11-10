using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Pedidos;

/// <summary>
/// Raiz de Agregação do pedido de compra de um jogo. Guarda o "retrato" do jogo
/// (título e valor) no momento da compra e controla a máquina de estados do pagamento.
/// </summary>
public sealed class Pedido : AggregateRoot<Guid>
{
    public const int TamanhoMaximoMotivo = 500;

    private Pedido() { }

    public Guid UsuarioId { get; private set; }
    public string EmailDoComprador { get; private set; } = string.Empty;
    public Guid JogoId { get; private set; }
    public string TituloDoJogo { get; private set; } = string.Empty;
    public Dinheiro Valor { get; private set; } = null!;
    public StatusDoPedido Status { get; private set; }
    public string? MotivoDaRecusa { get; private set; }
    public DateTime RealizadoEm { get; private set; }
    public DateTime? FinalizadoEm { get; private set; }

    public bool EstaAguardandoPagamento => Status == StatusDoPedido.AguardandoPagamento;

    /// <summary>Criado pelo <see cref="ServicoDeCompraDeJogos"/>, que valida as regras entre agregados.</summary>
    internal static Pedido Realizar(Guid usuarioId, string emailDoComprador, Guid jogoId, string tituloDoJogo, Dinheiro valor)
    {
        if (usuarioId == Guid.Empty)
            throw new DomainException("Comprador não identificado.");

        if (string.IsNullOrWhiteSpace(emailDoComprador))
            throw new DomainException("E-mail do comprador é obrigatório.");

        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            EmailDoComprador = emailDoComprador.Trim().ToLowerInvariant(),
            JogoId = jogoId,
            TituloDoJogo = tituloDoJogo,
            Valor = valor,
            Status = StatusDoPedido.AguardandoPagamento,
            RealizadoEm = DateTime.UtcNow
        };

        pedido.RegistrarEvento(new PedidoRealizado(
            pedido.Id, usuarioId, pedido.EmailDoComprador, jogoId, tituloDoJogo, valor.Valor));

        return pedido;
    }

    /// <summary>Confirma o pagamento. Idempotente: retorna false se o pedido já estava pago.</summary>
    public bool ConfirmarPagamento()
    {
        if (Status == StatusDoPedido.Pago)
            return false;

        if (Status != StatusDoPedido.AguardandoPagamento)
            throw new DomainException($"Pedido {Id} não pode ser pago: status atual {Status}.");

        Status = StatusDoPedido.Pago;
        FinalizadoEm = DateTime.UtcNow;
        RegistrarEvento(new PedidoPago(Id, UsuarioId, JogoId));
        return true;
    }

    /// <summary>Registra a recusa do pagamento. Idempotente: retorna false se já estava recusado.</summary>
    public bool RecusarPagamento(string? motivo)
    {
        if (Status == StatusDoPedido.PagamentoRecusado)
            return false;

        if (Status != StatusDoPedido.AguardandoPagamento)
            throw new DomainException($"Pedido {Id} não pode ser recusado: status atual {Status}.");

        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? "Pagamento recusado." : motivo.Trim();
        if (motivoNormalizado.Length > TamanhoMaximoMotivo)
            motivoNormalizado = motivoNormalizado[..TamanhoMaximoMotivo];

        Status = StatusDoPedido.PagamentoRecusado;
        MotivoDaRecusa = motivoNormalizado;
        FinalizadoEm = DateTime.UtcNow;
        RegistrarEvento(new PagamentoDoPedidoRecusado(Id, UsuarioId, JogoId, motivoNormalizado));
        return true;
    }
}
