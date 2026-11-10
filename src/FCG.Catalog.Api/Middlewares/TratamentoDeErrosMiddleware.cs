using System.Net;
using FCG.Catalog.Application.Exceptions;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Api.Middlewares;

/// <summary>
/// Tradução centralizada das exceções em respostas HTTP padronizadas.
/// Mantém os controllers livres de try/catch e de regras de negócio.
/// </summary>
public sealed class TratamentoDeErrosMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TratamentoDeErrosMiddleware> _logger;

    public TratamentoDeErrosMiddleware(RequestDelegate next, ILogger<TratamentoDeErrosMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await ResponderAsync(context, HttpStatusCode.BadRequest, ex.Message, ex);
        }
        catch (RecursoNaoEncontradoException ex)
        {
            await ResponderAsync(context, HttpStatusCode.NotFound, ex.Message, ex);
        }
        catch (ConflitoException ex)
        {
            await ResponderAsync(context, HttpStatusCode.Conflict, ex.Message, ex);
        }
        catch (NaoAutorizadoException ex)
        {
            await ResponderAsync(context, HttpStatusCode.Unauthorized, ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado em {Metodo} {Rota}", context.Request.Method, context.Request.Path);
            await ResponderAsync(context, HttpStatusCode.InternalServerError, "Erro interno. Contate o suporte.", null);
        }
    }

    private async Task ResponderAsync(HttpContext context, HttpStatusCode status, string mensagem, Exception? ex)
    {
        if (ex is not null)
            _logger.LogWarning("{Status} em {Metodo} {Rota}: {Mensagem}",
                (int)status, context.Request.Method, context.Request.Path, mensagem);

        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            status = (int)status,
            erro = mensagem,
            rastreio = context.TraceIdentifier
        });
    }
}
