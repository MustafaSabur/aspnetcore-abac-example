using Microsoft.AspNetCore.Authorization;

namespace AbacExample.Api.Authorization;

public static class PermissionPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder policy, string permission)
    {
        return policy.RequireAllPermissions(permission);
    }

    public static AuthorizationPolicyBuilder RequireAllPermissions(this AuthorizationPolicyBuilder policy, params string[] permissions)
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AppClaims.ProfileLoaded, BooleanClaimValues.True);
        policy.Requirements.Add(PermissionRequirement.All(permissions));
        return policy;
    }

    public static AuthorizationPolicyBuilder RequireAnyPermission(this AuthorizationPolicyBuilder policy, params string[] permissions)
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AppClaims.ProfileLoaded, BooleanClaimValues.True);
        policy.Requirements.Add(PermissionRequirement.Any(permissions));
        return policy;
    }

    public static TBuilder RequireAnyPermission<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAnyPermission(permissions)
            .Build();

        return builder.RequireAuthorization(policy);
    }
}
