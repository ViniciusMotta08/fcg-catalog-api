namespace FCG.Catalog.Application.DTOs;

public sealed record CadastrarJogoRequest(string Titulo, string? Descricao, string Genero, decimal Preco, DateTime DataLancamento);

public sealed record AtualizarJogoRequest(string Titulo, string? Descricao, string Genero, decimal Preco, DateTime DataLancamento);

public sealed record CriarPromocaoRequest(string Nome, decimal PercentualDesconto, DateTime DataInicio, DateTime DataFim);

public sealed record PromocaoResponse(Guid Id, string Nome, decimal PercentualDesconto, DateTime DataInicio, DateTime DataFim, bool Ativa);

public sealed record JogoResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    string Genero,
    decimal Preco,
    decimal PrecoVigente,
    DateTime DataLancamento,
    bool Ativo,
    IReadOnlyList<PromocaoResponse> Promocoes);
