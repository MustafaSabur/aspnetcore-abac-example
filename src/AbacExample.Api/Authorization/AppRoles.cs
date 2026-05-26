namespace AbacExample.Api.Authorization;

public static class AppRoles
{
    public const string DocumentAuthor = "document-author";
    public const string RecordsManager = "records-manager";
    public const string ComplianceAuditor = "compliance-auditor";

    public static readonly IReadOnlyDictionary<string, string[]> DefaultPermissions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [DocumentAuthor] =
            [
                AppPermissions.DocumentCreate,
                AppPermissions.DocumentRead,
                AppPermissions.DocumentUpdate
            ],
            [RecordsManager] =
            [
                AppPermissions.DocumentCreate,
                AppPermissions.DocumentRead,
                AppPermissions.DocumentUpdate,
                AppPermissions.DocumentDelete
            ],
            [ComplianceAuditor] = [AppPermissions.DocumentRead]
        };

    public static bool IsDefined(string roleName) =>
        DefaultPermissions.ContainsKey(roleName);

    public static IReadOnlyCollection<string> GetPermissionsForRoles(IEnumerable<string> roleNames)
    {
        return roleNames
            .SelectMany(roleName =>
                DefaultPermissions.TryGetValue(roleName, out var permissions)
                    ? permissions
                    : Array.Empty<string>())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
