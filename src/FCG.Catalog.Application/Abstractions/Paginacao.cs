namespace FCG.Catalog.Application.Abstractions;

/// <summary>Envelope padrão das listagens paginadas.</summary>
public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int Total)
{
    public int TotalPaginas => TamanhoPagina <= 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoPagina);
}

/// <summary>Normaliza os parâmetros de paginação vindos da requisição.</summary>
public static class Paginacao
{
    public const int TamanhoMaximo = 100;

    public static (int Pagina, int TamanhoPagina) Normalizar(int pagina, int tamanhoPagina)
    {
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1) tamanhoPagina = 20;
        if (tamanhoPagina > TamanhoMaximo) tamanhoPagina = TamanhoMaximo;
        return (pagina, tamanhoPagina);
    }
}
