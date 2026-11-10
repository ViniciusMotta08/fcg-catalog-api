using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Comum;

/// <summary>Value Object monetário (BRL): não negativo e arredondado em 2 casas.</summary>
public sealed record Dinheiro
{
    public static readonly Dinheiro Zero = new(0m);

    public decimal Valor { get; }

    private Dinheiro(decimal valor) => Valor = valor;

    public static Dinheiro Criar(decimal valor)
    {
        if (valor < 0)
            throw new DomainException("Valor monetário não pode ser negativo.");

        return new Dinheiro(Math.Round(valor, 2, MidpointRounding.AwayFromZero));
    }

    public Dinheiro AplicarDesconto(PercentualDeDesconto desconto) =>
        Criar(Valor - (Valor * desconto.Valor / 100m));

    public override string ToString() => $"R$ {Valor:0.00}";
}
