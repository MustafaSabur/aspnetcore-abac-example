using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AbacExample.Authorization;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public ClaimsPrincipal Principal =>
        httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No active HTTP context.");

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public Guid UserId =>
        Principal.UserId() ?? throw new InvalidOperationException("Missing app user id claim.");

    public Guid TenantId =>
        Principal.TenantId() ?? throw new InvalidOperationException("Missing tenant id claim.");

    public bool HasMfa => Principal.HasMfa();
}
