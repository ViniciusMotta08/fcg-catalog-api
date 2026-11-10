using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Exceptions;
using FCG.Catalog.Application.Mappers;
using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.Jogos;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Application.UseCases;

/// <summary>Casos de uso administrativos do catálogo (CRUD de jogos e promoções).</summary>
public sealed class GerenciarCatalogoUseCase
{
    private readonly IJogoRepository _jogos;
    private readonly IUnitOfWork _unitOfWork;

    public GerenciarCatalogoUseCase(IJogoRepository jogos, IUnitOfWork unitOfWork)
    {
        _jogos = jogos;
        _unitOfWork = unitOfWork;
    }

    public async Task<JogoResponse> CadastrarAsync(CadastrarJogoRequest request, CancellationToken ct = default)
    {
        if (await _jogos.ExisteComTituloAsync(request.Titulo?.Trim() ?? string.Empty, null, ct))
            throw new ConflitoException("Já existe um jogo cadastrado com este título.");

        var jogo = Jogo.Cadastrar(request.Titulo!, request.Descricao, request.Genero,
            Dinheiro.Criar(request.Preco), request.DataLancamento);

        await _jogos.AdicionarAsync(jogo, ct);
        await _unitOfWork.ConfirmarAsync(ct);

        return CatalogoMapper.ParaResponse(jogo);
    }

    public async Task<JogoResponse> AtualizarAsync(Guid id, AtualizarJogoRequest request, CancellationToken ct = default)
    {
        var jogo = await ObterAsync(id, ct);

        if (await _jogos.ExisteComTituloAsync(request.Titulo?.Trim() ?? string.Empty, id, ct))
            throw new ConflitoException("Já existe outro jogo cadastrado com este título.");

        jogo.Atualizar(request.Titulo!, request.Descricao, request.Genero, Dinheiro.Criar(request.Preco), request.DataLancamento);
        await _unitOfWork.ConfirmarAsync(ct);

        return CatalogoMapper.ParaResponse(jogo);
    }

    public async Task DesativarAsync(Guid id, CancellationToken ct = default)
    {
        var jogo = await ObterAsync(id, ct);
        jogo.Desativar();
        await _unitOfWork.ConfirmarAsync(ct);
    }

    public async Task AtivarAsync(Guid id, CancellationToken ct = default)
    {
        var jogo = await ObterAsync(id, ct);
        jogo.Ativar();
        await _unitOfWork.ConfirmarAsync(ct);
    }

    public async Task<PromocaoResponse> CriarPromocaoAsync(Guid jogoId, CriarPromocaoRequest request, CancellationToken ct = default)
    {
        var jogo = await ObterAsync(jogoId, ct);

        var promocao = jogo.AdicionarPromocao(
            request.Nome,
            PercentualDeDesconto.Criar(request.PercentualDesconto),
            PeriodoDeVigencia.Criar(request.DataInicio, request.DataFim));

        await _unitOfWork.ConfirmarAsync(ct);
        return CatalogoMapper.ParaResponse(promocao);
    }

    public async Task EncerrarPromocaoAsync(Guid jogoId, Guid promocaoId, CancellationToken ct = default)
    {
        var jogo = await ObterAsync(jogoId, ct);
        jogo.EncerrarPromocao(promocaoId);
        await _unitOfWork.ConfirmarAsync(ct);
    }

    private async Task<Jogo> ObterAsync(Guid id, CancellationToken ct) =>
        await _jogos.ObterPorIdAsync(id, ct)
        ?? throw new RecursoNaoEncontradoException($"Jogo {id} não encontrado.");
}
