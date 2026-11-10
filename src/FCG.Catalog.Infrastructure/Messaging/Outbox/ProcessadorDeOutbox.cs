using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Infrastructure.Messaging.Outbox;

/// <summary>
/// Worker que lê as mensagens pendentes da Outbox e as publica no RabbitMQ.
/// Entrega "at-least-once": os consumidores são idempotentes.
/// </summary>
public sealed class ProcessadorDeOutbox<TContext> : BackgroundService where TContext : DbContext
{
    private const int TamanhoDoLote = 50;
    private const int MaximoDeTentativas = 10;
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(3);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PublicadorRabbitMq _publicador;
    private readonly ILogger<ProcessadorDeOutbox<TContext>> _logger;

    public ProcessadorDeOutbox(
        IServiceScopeFactory scopeFactory,
        PublicadorRabbitMq publicador,
        ILogger<ProcessadorDeOutbox<TContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _publicador = publicador;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarLoteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // Banco ainda subindo ou indisponível: tenta de novo no próximo ciclo.
                _logger.LogWarning(ex, "Falha ao ler a Outbox. Nova tentativa em {Intervalo}.", Intervalo);
            }

            try { await Task.Delay(Intervalo, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task ProcessarLoteAsync(CancellationToken ct)
    {
        using var escopo = _scopeFactory.CreateScope();
        var context = escopo.ServiceProvider.GetRequiredService<TContext>();

        var pendentes = await context.Set<OutboxMessage>()
            .Where(m => m.ProcessadaEm == null && m.Tentativas < MaximoDeTentativas)
            .OrderBy(m => m.OcorreuEm)
            .Take(TamanhoDoLote)
            .ToListAsync(ct);

        if (pendentes.Count == 0)
            return;

        foreach (var mensagem in pendentes)
        {
            try
            {
                _publicador.Publicar(mensagem);
                mensagem.MarcarComoProcessada();
            }
            catch (Exception ex)
            {
                mensagem.RegistrarFalha(ex.Message);
                _logger.LogWarning(ex, "Falha ao publicar {Tipo} {EventId} (tentativa {Tentativa}).",
                    mensagem.Tipo, mensagem.Id, mensagem.Tentativas);

                // Broker fora: interrompe o lote e preserva a ordem para o próximo ciclo.
                break;
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
