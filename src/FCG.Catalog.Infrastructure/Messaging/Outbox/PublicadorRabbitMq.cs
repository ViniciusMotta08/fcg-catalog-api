using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FCG.Catalog.Infrastructure.Messaging.Outbox;

/// <summary>
/// Publica mensagens no exchange de eventos com confirmação do broker (publisher confirms)
/// e mensagens persistentes. Mantém um único canal protegido por lock.
/// </summary>
public sealed class PublicadorRabbitMq : IDisposable
{
    private static readonly TimeSpan TempoMaximoConfirmacao = TimeSpan.FromSeconds(5);

    private readonly RabbitMqConexao _conexao;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<PublicadorRabbitMq> _logger;
    private readonly object _lock = new();
    private IModel? _canal;

    public PublicadorRabbitMq(RabbitMqConexao conexao, IOptions<RabbitMqOptions> options, ILogger<PublicadorRabbitMq> logger)
    {
        _conexao = conexao;
        _options = options.Value;
        _logger = logger;
    }

    public void Publicar(OutboxMessage mensagem)
    {
        lock (_lock)
        {
            var canal = ObterCanal();

            var propriedades = canal.CreateBasicProperties();
            propriedades.Persistent = true;
            propriedades.ContentType = "application/json";
            propriedades.MessageId = mensagem.Id.ToString();
            propriedades.Type = mensagem.Tipo;
            propriedades.Timestamp = new AmqpTimestamp(
                new DateTimeOffset(DateTime.SpecifyKind(mensagem.OcorreuEm, DateTimeKind.Utc)).ToUnixTimeSeconds());

            canal.BasicPublish(
                exchange: _options.Exchange,
                routingKey: mensagem.RoutingKey,
                mandatory: false,
                basicProperties: propriedades,
                body: Encoding.UTF8.GetBytes(mensagem.Conteudo));

            canal.WaitForConfirmsOrDie(TempoMaximoConfirmacao);

            _logger.LogInformation("Evento {Tipo} {EventId} publicado com routing key {RoutingKey}.",
                mensagem.Tipo, mensagem.Id, mensagem.RoutingKey);
        }
    }

    private IModel ObterCanal()
    {
        if (_canal is { IsOpen: true })
            return _canal;

        _canal?.Dispose();
        _canal = _conexao.CriarCanal();
        _canal.ConfirmSelect();
        RabbitMqTopologia.DeclararExchanges(_canal, _options);
        return _canal;
    }

    public void Dispose()
    {
        try
        {
            _canal?.Close();
            _canal?.Dispose();
        }
        catch
        {
            // Encerramento do host: nada a fazer.
        }
    }
}
