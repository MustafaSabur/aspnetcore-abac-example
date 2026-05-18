using System.Security.Claims;

namespace AbacExample.Api.Authorization;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid TenantId { get; }
    bool HasMfa { get; }
    ClaimsPrincipal Principal { get; }
}
