using Microsoft.AspNetCore.Authorization;

namespace AbacExample.Api.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userPermissions = context.User
            .FindAll(AppClaims.Permission)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allowed = requirement.MatchMode switch
        {
            PermissionMatchMode.All => requirement.Permissions.All(userPermissions.Contains),
            PermissionMatchMode.Any => requirement.Permissions.Any(userPermissions.Contains),
            _ => false
        };

        if (allowed)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
