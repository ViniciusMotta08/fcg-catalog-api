using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FCG.Catalog.Infrastructure.Messaging;

/// <summary>Readiness do broker: o pod só recebe tráfego quando consegue falar com o RabbitMQ.</summary>
public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqConexao _conexao;

    public RabbitMqHealthCheck(RabbitMqConexao conexao) => _conexao = conexao;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var canal = _conexao.CriarCanal();
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ acessível."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ indisponível.", ex));
        }
    }
}
