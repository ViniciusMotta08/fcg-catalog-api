namespace FCG.Catalog.Domain.SeedWork;

/// <summary>
/// Contrato de toda Raiz de Agregação: expõe os eventos de domínio ocorridos
/// para que a infraestrutura os publique ao confirmar a transação.
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> EventosDeDominio { get; }
    void LimparEventosDeDominio();
}

/// <summary>
/// Raiz de Agregação: único ponto de entrada do agregado. Garante as invariantes
/// e registra os eventos de domínio decorrentes das suas mudanças de estado.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : notnull
{
    private readonly List<IDomainEvent> _eventosDeDominio = new();

    public IReadOnlyCollection<IDomainEvent> EventosDeDominio => _eventosDeDominio.AsReadOnly();

    protected void RegistrarEvento(IDomainEvent evento) => _eventosDeDominio.Add(evento);

    public void LimparEventosDeDominio() => _eventosDeDominio.Clear();
}
