using RabbitMQ.Client;

namespace FCG.Catalog.Infrastructure.Messaging;

/// <summary>
/// Declaração idempotente da topologia: exchange de eventos (topic), exchange de dead letter
/// e, para cada fila consumida, a fila principal e sua fila ".dlq".
/// </summary>
public static class RabbitMqTopologia
{
    public static void DeclararExchanges(IModel canal, RabbitMqOptions options)
    {
        canal.ExchangeDeclare(options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        canal.ExchangeDeclare(options.DeadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false);
    }

    public static void DeclararFila(IModel canal, RabbitMqOptions options, string fila, string routingKey)
    {
        var filaDlq = $"{fila}.dlq";

        canal.QueueDeclare(filaDlq, durable: true, exclusive: false, autoDelete: false, arguments: null);
        canal.QueueBind(filaDlq, options.DeadLetterExchange, routingKey: fila);

        var argumentos = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = options.DeadLetterExchange,
            ["x-dead-letter-routing-key"] = fila
        };

        canal.QueueDeclare(fila, durable: true, exclusive: false, autoDelete: false, arguments: argumentos);
        canal.QueueBind(fila, options.Exchange, routingKey);
    }
}
