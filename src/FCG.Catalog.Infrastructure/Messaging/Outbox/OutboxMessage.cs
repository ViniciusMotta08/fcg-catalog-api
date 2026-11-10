using FCG.Catalog.Application.Abstractions;

namespace FCG.Catalog.Infrastructure.Messaging.Outbox;

/// <summary>
/// Transactional Outbox: o evento de integração é gravado na mesma transação do agregado
/// e publicado depois no RabbitMQ. Assim nenhum evento se perde se o broker estiver fora do ar.
/// </summary>
public sealed class OutboxMessage
{
    public const int TamanhoMaximoErro = 2000;

    private OutboxMessage() { }

    public Guid Id { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string RoutingKey { get; private set; } = string.Empty;
    public string Conteudo { get; private set; } = string.Empty;
    public DateTime OcorreuEm { get; private set; }
    public DateTime? ProcessadaEm { get; private set; }
    public int Tentativas { get; private set; }
    public string? UltimoErro { get; private set; }

    public static OutboxMessage Criar(IntegrationEvent evento) => new()
    {
        Id = evento.EventId,
        Tipo = evento.GetType().Name,
        RoutingKey = evento.RoutingKey,
        Conteudo = SerializadorDeEventos.Serializar(evento),
        OcorreuEm = evento.OccurredAt
    };

    public void MarcarComoProcessada()
    {
        ProcessadaEm = DateTime.UtcNow;
        UltimoErro = null;
    }

    public void RegistrarFalha(string erro)
    {
        Tentativas++;
        UltimoErro = erro.Length > TamanhoMaximoErro ? erro[..TamanhoMaximoErro] : erro;
    }
}
