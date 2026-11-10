namespace FCG.Catalog.Application.Exceptions;

/// <summary>Recurso solicitado não existe. Traduzido para HTTP 404.</summary>
public class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string mensagem) : base(mensagem) { }
}

/// <summary>Operação conflita com o estado atual (ex.: duplicidade). Traduzido para HTTP 409.</summary>
public class ConflitoException : Exception
{
    public ConflitoException(string mensagem) : base(mensagem) { }
}

/// <summary>Credenciais inválidas ou acesso negado. Traduzido para HTTP 401.</summary>
public class NaoAutorizadoException : Exception
{
    public NaoAutorizadoException(string mensagem) : base(mensagem) { }
}
