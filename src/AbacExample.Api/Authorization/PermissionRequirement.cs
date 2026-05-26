using Microsoft.AspNetCore.Authorization;

namespace AbacExample.Api.Authorization;

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
