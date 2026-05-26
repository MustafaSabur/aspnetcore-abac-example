namespace AbacExample.Authorization;

public static class PermissionPolicyNames
{
    private const string AnyPermissionPrefix = "abac:any:";

    public static string ForAny(IEnumerable<string> permissions)
    {
        var encodedPermissions = permissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(Uri.EscapeDataString)
            .ToArray();

        return AnyPermissionPrefix + string.Join(",", encodedPermissions);
    }

    public static bool TryParseAny(string policyName, out IReadOnlyCollection<string> permissions)
    {
        permissions = [];

        if (!policyName.StartsWith(AnyPermissionPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        permissions = policyName[AnyPermissionPrefix.Length..]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Uri.UnescapeDataString)
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return permissions.Count > 0;
    }
}
