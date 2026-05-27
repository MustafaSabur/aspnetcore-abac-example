namespace AbacExample.Api.Authorization;

public static class DocumentRoles
{
    public const string DocumentAuthor = "document-author";
    public const string RecordsManager = "records-manager";
    public const string ComplianceAuditor = "compliance-auditor";

    public static readonly IReadOnlyDictionary<string, string[]> DefaultPermissions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [DocumentAuthor] =
            [
                DocumentPermissions.DocumentCreate,
                DocumentPermissions.DocumentRead,
                DocumentPermissions.DocumentUpdate
            ],
            [RecordsManager] =
            [
                DocumentPermissions.DocumentCreate,
                DocumentPermissions.DocumentRead,
                DocumentPermissions.DocumentUpdate,
                DocumentPermissions.DocumentDelete
            ],
            [ComplianceAuditor] = [DocumentPermissions.DocumentRead]
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
