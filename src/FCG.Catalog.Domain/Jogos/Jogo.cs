using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Jogos;

/// <summary>
/// Raiz de Agregação do Catálogo. Garante as regras do jogo e das suas promoções,
/// e calcula o preço vigente em uma data.
/// </summary>
public sealed class Jogo : AggregateRoot<Guid>
{
    public const int TamanhoMaximoTitulo = 200;
    public const int TamanhoMaximoDescricao = 2000;
    public const int TamanhoMaximoGenero = 80;

    private readonly List<Promocao> _promocoes = new();

    private Jogo() { }

    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Genero { get; private set; } = string.Empty;
    public Dinheiro Preco { get; private set; } = null!;
    public DateTime DataLancamento { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CadastradoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    public IReadOnlyCollection<Promocao> Promocoes => _promocoes.AsReadOnly();

    public static Jogo Cadastrar(string titulo, string? descricao, string genero, Dinheiro preco, DateTime dataLancamento)
    {
        var jogo = new Jogo { Id = Guid.NewGuid(), Ativo = true, CadastradoEm = DateTime.UtcNow };
        jogo.DefinirDados(titulo, descricao, genero, preco, dataLancamento);
        return jogo;
    }

    public void Atualizar(string titulo, string? descricao, string genero, Dinheiro preco, DateTime dataLancamento)
    {
        DefinirDados(titulo, descricao, genero, preco, dataLancamento);
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new DomainException("Jogo já está inativo.");

        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Ativar()
    {
        if (Ativo)
            throw new DomainException("Jogo já está ativo.");

        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>Cria uma promoção. Não permite promoções ativas com períodos sobrepostos.</summary>
    public Promocao AdicionarPromocao(string nome, PercentualDeDesconto desconto, PeriodoDeVigencia vigencia)
    {
        if (!Ativo)
            throw new DomainException("Não é possível criar promoção para um jogo inativo.");

        var sobreposta = _promocoes.Any(p => p.Ativa
                                             && p.Vigencia.Inicio <= vigencia.Fim
                                             && vigencia.Inicio <= p.Vigencia.Fim);
        if (sobreposta)
            throw new DomainException("Já existe uma promoção ativa neste período para o jogo.");

        var promocao = Promocao.Criar(nome, desconto, vigencia);
        _promocoes.Add(promocao);
        AtualizadoEm = DateTime.UtcNow;
        return promocao;
    }

    public void EncerrarPromocao(Guid promocaoId)
    {
        var promocao = _promocoes.FirstOrDefault(p => p.Id == promocaoId)
                       ?? throw new DomainException("Promoção não encontrada para este jogo.");

        promocao.Encerrar();
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>Preço a ser cobrado na data informada (com a promoção vigente, se houver).</summary>
    public Dinheiro ObterPrecoVigente(DateTime data)
    {
        var promocao = _promocoes
            .Where(p => p.EstaVigenteEm(data))
            .OrderByDescending(p => p.Desconto.Valor)
            .FirstOrDefault();

        return promocao is null ? Preco : Preco.AplicarDesconto(promocao.Desconto);
    }

    private void DefinirDados(string titulo, string? descricao, string genero, Dinheiro preco, DateTime dataLancamento)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Trim().Length < 2)
            throw new DomainException("Título do jogo é obrigatório e deve ter ao menos 2 caracteres.");

        if (titulo.Trim().Length > TamanhoMaximoTitulo)
            throw new DomainException($"Título deve ter no máximo {TamanhoMaximoTitulo} caracteres.");

        if (string.IsNullOrWhiteSpace(genero))
            throw new DomainException("Gênero do jogo é obrigatório.");

        if (genero.Trim().Length > TamanhoMaximoGenero)
            throw new DomainException($"Gênero deve ter no máximo {TamanhoMaximoGenero} caracteres.");

        if (descricao?.Trim().Length > TamanhoMaximoDescricao)
            throw new DomainException($"Descrição deve ter no máximo {TamanhoMaximoDescricao} caracteres.");

        if (preco is null || preco.Valor <= 0)
            throw new DomainException("Preço do jogo deve ser maior que zero.");

        Titulo = titulo.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
        Genero = genero.Trim();
        Preco = preco;
        DataLancamento = dataLancamento;
    }
}
