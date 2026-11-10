namespace FCG.Catalog.Domain.SeedWork;

/// <summary>Violação de uma regra de negócio (invariante) do domínio.</summary>
public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem) { }
}
