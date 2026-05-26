using AbacExample.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace AbacExample.Api.Authorization;

public static class AppPermissionPolicyRegistration
{
    public static AuthorizationBuilder AddAppPermissionPolicies(this AuthorizationBuilder builder)
    {
        foreach (var permission in AppPermissions.All)
        {
            builder.AddPolicy(permission, policy => policy.RequirePermission(permission));
        }

        return builder;
    }
}
