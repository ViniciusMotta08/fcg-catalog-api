using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Jogos;

/// <summary>
/// Entidade interna do agregado Jogo. Só é criada/alterada através da raiz (<see cref="Jogo"/>).
/// </summary>
public sealed class Promocao : Entity<Guid>
{
    public const int TamanhoMaximoNome = 120;

    private Promocao() { }

    public string Nome { get; private set; } = string.Empty;
    public PercentualDeDesconto Desconto { get; private set; } = null!;
    public PeriodoDeVigencia Vigencia { get; private set; } = null!;
    public bool Ativa { get; private set; }

    internal static Promocao Criar(string nome, PercentualDeDesconto desconto, PeriodoDeVigencia vigencia)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome da promoção é obrigatório.");

        if (nome.Trim().Length > TamanhoMaximoNome)
            throw new DomainException($"Nome da promoção deve ter no máximo {TamanhoMaximoNome} caracteres.");

        return new Promocao
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Desconto = desconto ?? throw new DomainException("Desconto é obrigatório."),
            Vigencia = vigencia ?? throw new DomainException("Período de vigência é obrigatório."),
            Ativa = true
        };
    }

    public bool EstaVigenteEm(DateTime data) => Ativa && Vigencia.Contem(data);

    internal void Encerrar() => Ativa = false;
}
