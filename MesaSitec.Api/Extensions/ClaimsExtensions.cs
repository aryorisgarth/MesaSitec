using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MesaSitec.Application.Exceptions;
using MesaSitec.Application.Services;
using MesaSitec.Domain.Enums;

namespace MesaSitec.Api.Extensions;

public static class ClaimsExtensions
{
    public static CurrentUser GetCurrentUser(this ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var tenantClaim = user.FindFirst("tenantId")?.Value;
        var rolClaim = user.FindFirst("rol")?.Value;
        var emailClaim = user.FindFirst("email")?.Value;

        if (idClaim is null || tenantClaim is null || rolClaim is null || emailClaim is null)
            throw AppException.NoAutenticado();

        if (!Guid.TryParse(idClaim, out var id) || !Guid.TryParse(tenantClaim, out var tenantId))
            throw AppException.NoAutenticado();

        if (!Enum.TryParse<Rol>(rolClaim, ignoreCase: true, out var rol))
            throw AppException.NoAutenticado();

        return new CurrentUser(id, tenantId, rol, emailClaim);
    }
}
