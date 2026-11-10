namespace FCG.Catalog.Infrastructure.Messaging;

/// <summary>
/// Configuração do broker. Valores não sensíveis vêm do ConfigMap
/// e as credenciais (Usuario/Senha) vêm do Secret.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string Secao = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Porta { get; set; } = 5672;
    public string Usuario { get; set; } = "guest";
    public string Senha { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    /// <summary>Exchange do tipo topic onde todos os eventos de integração são publicados.</summary>
    public string Exchange { get; set; } = "fcg.events";

    /// <summary>Exchange que recebe as mensagens rejeitadas (dead letter).</summary>
    public string DeadLetterExchange { get; set; } = "fcg.events.dlx";

    public ushort PrefetchCount { get; set; } = 10;

    /// <summary>Nomes das filas consumidas por este serviço (chave lógica → nome da fila).</summary>
    public Dictionary<string, string> Filas { get; set; } = new();

    public string ObterFila(string chave) =>
        Filas.TryGetValue(chave, out var fila) && !string.IsNullOrWhiteSpace(fila)
            ? fila
            : throw new InvalidOperationException($"Fila '{chave}' não configurada em RabbitMq:Filas.");
}
