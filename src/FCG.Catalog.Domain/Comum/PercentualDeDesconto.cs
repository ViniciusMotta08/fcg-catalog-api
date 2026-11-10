using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Comum;

/// <summary>Value Object do percentual de desconto de uma promoção (entre 1% e 90%).</summary>
public sealed record PercentualDeDesconto
{
    public const decimal Minimo = 1m;
    public const decimal Maximo = 90m;

    public decimal Valor { get; }

    private PercentualDeDesconto(decimal valor) => Valor = valor;

    public static PercentualDeDesconto Criar(decimal valor)
    {
        if (valor < Minimo || valor > Maximo)
            throw new DomainException($"Percentual de desconto deve estar entre {Minimo} e {Maximo}.");

        return new PercentualDeDesconto(valor);
    }
}
