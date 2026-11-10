using System.Text;
using FCG.Catalog.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FCG.Catalog.Infrastructure.Messaging;

/// <summary>
/// Adaptador de entrada da mensageria: consome uma fila, desserializa o evento de integração
/// e delega ao caso de uso (<see cref="IIntegrationEventHandler{TEvento}"/>) em um escopo de DI próprio.
/// Falha na 1ª entrega → reenfileira uma vez; falha na reentrega → vai para a fila ".dlq".
/// </summary>
public abstract class ConsumidorDeEventoBase<TEvento> : BackgroundService where TEvento : IntegrationEvent
{
    private readonly RabbitMqConexao _conexao;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private IModel? _canal;

    protected ConsumidorDeEventoBase(
        RabbitMqConexao conexao,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger logger)
    {
        _conexao = conexao;
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Chave da fila em RabbitMq:Filas (ex.: "OrderPlaced").</summary>
    protected abstract string ChaveDaFila { get; }

    /// <summary>Routing key do evento consumido (ex.: "order.placed").</summary>
    protected abstract string RoutingKey { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Libera a inicialização do host: a conexão com o broker não deve atrasar a subida da API.
        await Task.Yield();

        // O broker pode subir depois da API: tenta até conseguir iniciar o consumo.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IniciarConsumo(stoppingToken);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível iniciar o consumo de {Evento}. Nova tentativa em 5s.",
                    typeof(TEvento).Name);

                try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
                catch (OperationCanceledException) { return; }
            }
        }
    }

    private void IniciarConsumo(CancellationToken stoppingToken)
    {
        var fila = _options.ObterFila(ChaveDaFila);

        _canal = _conexao.CriarCanal();
        RabbitMqTopologia.DeclararExchanges(_canal, _options);
        RabbitMqTopologia.DeclararFila(_canal, _options, fila, RoutingKey);
        _canal.BasicQos(prefetchSize: 0, prefetchCount: _options.PrefetchCount, global: false);

        var consumidor = new AsyncEventingBasicConsumer(_canal);
        consumidor.Received += (_, entrega) => ProcessarAsync(entrega, stoppingToken);

        _canal.BasicConsume(queue: fila, autoAck: false, consumer: consumidor);

        _logger.LogInformation("Consumindo {Evento} na fila {Fila} (routing key {RoutingKey}).",
            typeof(TEvento).Name, fila, RoutingKey);
    }

    private async Task ProcessarAsync(BasicDeliverEventArgs entrega, CancellationToken ct)
    {
        var canal = _canal!;
        var json = Encoding.UTF8.GetString(entrega.Body.Span);

        try
        {
            var evento = SerializadorDeEventos.Desserializar<TEvento>(json)
                         ?? throw new InvalidOperationException("Mensagem vazia ou inválida.");

            using (_logger.BeginScope(new Dictionary<string, object> { ["EventId"] = evento.EventId }))
            {
                _logger.LogInformation("Evento {Evento} {EventId} recebido.", typeof(TEvento).Name, evento.EventId);

                using var escopo = _scopeFactory.CreateScope();
                var handler = escopo.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TEvento>>();
                await handler.TratarAsync(evento, ct);
            }

            canal.BasicAck(entrega.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            var reenfileirar = !entrega.Redelivered;

            _logger.LogError(ex,
                "Falha ao processar {Evento} (reentrega: {Reentrega}). {Destino}. Conteúdo: {Conteudo}",
                typeof(TEvento).Name, entrega.Redelivered,
                reenfileirar ? "Mensagem reenfileirada" : "Mensagem enviada para a DLQ", json);

            canal.BasicNack(entrega.DeliveryTag, multiple: false, requeue: reenfileirar);
        }
    }

    public override void Dispose()
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

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
