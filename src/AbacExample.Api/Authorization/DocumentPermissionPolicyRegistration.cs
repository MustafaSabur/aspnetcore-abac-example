using AbacExample.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace AbacExample.Api.Authorization;

public static class DocumentPermissionPolicyRegistration
{
    public static AuthorizationBuilder AddDocumentPermissionPolicies(this AuthorizationBuilder builder)
    {
        foreach (var permission in DocumentPermissions.All)
        {
            builder.AddPolicy(permission, policy => policy.RequirePermission(permission));
        }

        return builder;
    }
}
