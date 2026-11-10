using FCG.Catalog.Application.Abstractions;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Exceptions;
using FCG.Catalog.Application.Mappers;
using FCG.Catalog.Domain.Jogos;

namespace FCG.Catalog.Application.UseCases;

/// <summary>Consultas do catálogo (somente leitura).</summary>
public sealed class ConsultarCatalogoUseCase
{
    private readonly IJogoRepository _jogos;

    public ConsultarCatalogoUseCase(IJogoRepository jogos) => _jogos = jogos;

    public async Task<JogoResponse> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var jogo = await _jogos.ObterPorIdAsync(id, ct)
                   ?? throw new RecursoNaoEncontradoException($"Jogo {id} não encontrado.");

        return CatalogoMapper.ParaResponse(jogo);
    }

    public async Task<ResultadoPaginado<JogoResponse>> ListarAsync(
        int pagina, int tamanhoPagina, bool apenasAtivos, CancellationToken ct = default)
    {
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);

        var jogos = await _jogos.ListarAsync(pagina, tamanhoPagina, apenasAtivos, ct);
        var total = await _jogos.ContarAsync(apenasAtivos, ct);

        return new ResultadoPaginado<JogoResponse>(
            jogos.Select(CatalogoMapper.ParaResponse).ToList(), pagina, tamanhoPagina, total);
    }
}
