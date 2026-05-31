using System.Security.Claims;
using TaskFlowPro.Application.Exceptions;

namespace TaskFlowPro.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetCurrentUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedException("Token inválido.");

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Token inválido.");

        return userId;
    }
}