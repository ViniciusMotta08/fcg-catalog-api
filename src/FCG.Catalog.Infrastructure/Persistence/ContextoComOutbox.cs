using FCG.Catalog.Application.Abstractions;
using FCG.Catalog.Domain.SeedWork;
using FCG.Catalog.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Persistence;

/// <summary>
/// Base do DbContext do serviço. Implementa a Unidade de Trabalho: ao confirmar,
/// converte os eventos de domínio dos agregados rastreados em mensagens da Outbox,
/// gravadas na MESMA transação das mudanças do agregado.
/// </summary>
public abstract class ContextoComOutbox : DbContext, IUnitOfWork
{
    private readonly IIntegrationEventMapper _mapeador;

    protected ContextoComOutbox(DbContextOptions options, IIntegrationEventMapper mapeador) : base(options)
    {
        _mapeador = mapeador;
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public async Task<int> ConfirmarAsync(CancellationToken ct = default)
    {
        var agregados = ChangeTracker.Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.EventosDeDominio.Count > 0)
            .ToList();

        var eventosDeDominio = agregados.SelectMany(a => a.EventosDeDominio).ToList();
        agregados.ForEach(a => a.LimparEventosDeDominio());

        foreach (var eventoDeIntegracao in eventosDeDominio.SelectMany(_mapeador.Mapear))
            OutboxMessages.Add(OutboxMessage.Criar(eventoDeIntegracao));

        return await SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContextoComOutbox).Assembly);

        // Eventos de domínio são estado transitório em memória — nunca são mapeados como coluna.
        foreach (var tipo in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(tipo.ClrType).Ignore(nameof(IAggregateRoot.EventosDeDominio));
        }

        base.OnModelCreating(modelBuilder);
    }
}
