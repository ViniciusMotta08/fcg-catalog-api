namespace FCG.Catalog.Domain.SeedWork;

/// <summary>
/// Entidade: possui identidade única e ciclo de vida próprio.
/// Duas entidades são iguais quando têm o mesmo tipo e o mesmo identificador.
/// </summary>
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> outra || outra.GetType() != GetType())
            return false;

        if (ReferenceEquals(this, outra))
            return true;

        return !EqualityComparer<TId>.Default.Equals(Id, default!) && Id.Equals(outra.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? esquerda, Entity<TId>? direita) =>
        esquerda is null ? direita is null : esquerda.Equals(direita);

    public static bool operator !=(Entity<TId>? esquerda, Entity<TId>? direita) => !(esquerda == direita);
}
