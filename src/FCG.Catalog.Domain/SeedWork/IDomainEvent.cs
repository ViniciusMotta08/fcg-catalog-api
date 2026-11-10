namespace FCG.Catalog.Domain.SeedWork;

/// <summary>Fato relevante que aconteceu no domínio, descrito no passado.</summary>
public interface IDomainEvent
{
    DateTime OcorreuEm { get; }
}

/// <summary>Base dos eventos de domínio (imutáveis por serem records).</summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OcorreuEm { get; init; } = DateTime.UtcNow;
}
