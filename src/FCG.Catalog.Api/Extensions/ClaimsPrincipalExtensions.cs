using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FCG.Catalog.Api.Extensions;

/// <summary>Leitura dos claims do JWT emitido pelo UsersAPI.</summary>
public static class ClaimsPrincipalExtensions
{
    public const string PerfilAdministrador = "Administrador";

    public static Guid ObterUsuarioId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? usuario.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(valor, out var id) ? id : Guid.Empty;
    }

    public static string ObterEmail(this ClaimsPrincipal usuario) =>
        usuario.FindFirst(ClaimTypes.Email)?.Value
        ?? usuario.FindFirst(JwtRegisteredClaimNames.Email)?.Value
        ?? string.Empty;

    /// <summary>Identificação usada nos campos de auditoria.</summary>
    public static string ObterIdentificacao(this ClaimsPrincipal usuario)
    {
        var email = usuario.ObterEmail();
        return string.IsNullOrWhiteSpace(email) ? "sistema" : email;
    }
}
