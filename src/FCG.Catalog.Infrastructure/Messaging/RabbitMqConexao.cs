using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FCG.Catalog.Infrastructure.Messaging;

/// <summary>
/// Conexão única (singleton) com o RabbitMQ, compartilhada por publicadores e consumidores.
/// Usa a recuperação automática do RabbitMQ.Client em quedas de rede.
/// </summary>
public sealed class RabbitMqConexao : IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqConexao> _logger;
    private readonly object _lock = new();
    private IConnection? _conexao;

    public RabbitMqConexao(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConexao> logger)
    {
        _logger = logger;
        var config = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Porta,
            UserName = config.Usuario,
            Password = config.Senha,
            VirtualHost = config.VirtualHost,
            ClientProvidedName = "FCG.Catalog",
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
        };
    }

    public bool EstaConectado => _conexao is { IsOpen: true };

    /// <summary>Cria um canal (IModel). Canais não são thread-safe: cada usuário mantém o seu.</summary>
    public IModel CriarCanal()
    {
        GarantirConexao();
        return _conexao!.CreateModel();
    }

    private void GarantirConexao()
    {
        if (EstaConectado)
            return;

        lock (_lock)
        {
            if (EstaConectado)
                return;

            _logger.LogInformation("Conectando ao RabbitMQ em {Host}:{Porta}...", _factory.HostName, _factory.Port);
            _conexao = _factory.CreateConnection();
            _logger.LogInformation("Conectado ao RabbitMQ.");
        }
    }

    public void Dispose()
    {
        try
        {
            _conexao?.Close();
            _conexao?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao fechar a conexão com o RabbitMQ.");
        }
    }
}
