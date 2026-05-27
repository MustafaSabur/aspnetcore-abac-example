using Microsoft.AspNetCore.Authorization;

namespace AbacExample.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userPermissions = context.User
            .FindAll(AuthorizationClaims.Permission)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (requirement.Permissions.Any(userPermissions.Contains))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
