using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace AbacExample.Api.Authorization;

public static class AppPermissions
{
    public const string DocumentCreate = "documents:create";
    public const string DocumentRead = "documents:read";
    public const string DocumentUpdate = "documents:update";
    public const string DocumentDelete = "documents:delete";

    public static readonly IReadOnlyCollection<string> All =
    [
        DocumentCreate,
        DocumentRead,
        DocumentUpdate,
        DocumentDelete
    ];
}

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

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    private PermissionRequirement(PermissionMatchMode matchMode, IEnumerable<string> permissions)
    {
        MatchMode = matchMode;
        Permissions = permissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public PermissionMatchMode MatchMode { get; }

    public IReadOnlyCollection<string> Permissions { get; }

    public static PermissionRequirement All(IEnumerable<string> permissions) =>
        new(PermissionMatchMode.All, permissions);

    public static PermissionRequirement Any(IEnumerable<string> permissions) =>
        new(PermissionMatchMode.Any, permissions);
}

public enum PermissionMatchMode
{
    All = 1,
    Any = 2
}

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
