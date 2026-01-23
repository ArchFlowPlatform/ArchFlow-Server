using System.Security.Claims;

namespace archFlowServer.Utils.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claimValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(claimValue))
            throw new UnauthorizedAccessException("usuário não autenticado.");

        if (!Guid.TryParse(claimValue, out var userId))
            throw new UnauthorizedAccessException("Identificador de usuário inválido.");

        return userId;
    }
}
