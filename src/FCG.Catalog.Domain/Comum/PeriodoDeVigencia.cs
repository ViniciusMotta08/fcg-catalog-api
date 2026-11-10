using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Comum;

/// <summary>Value Object de intervalo de datas (início inclusivo, fim inclusivo).</summary>
public sealed record PeriodoDeVigencia
{
    public DateTime Inicio { get; private init; }
    public DateTime Fim { get; private init; }

    // Exigido pelo EF Core (tipo owned).
    private PeriodoDeVigencia() { }

    public static PeriodoDeVigencia Criar(DateTime inicio, DateTime fim)
    {
        if (fim <= inicio)
            throw new DomainException("A data final deve ser maior que a data inicial.");

        return new PeriodoDeVigencia { Inicio = inicio, Fim = fim };
    }

    public bool Contem(DateTime data) => data >= Inicio && data <= Fim;
}
