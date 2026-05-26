namespace AbacExample.Api.Authorization;

public static class AppRoles
{
    public const string CaseAgent = "case-agent";
    public const string CaseSupervisor = "case-supervisor";
    public const string Auditor = "auditor";

    public static readonly IReadOnlyDictionary<string, string[]> DefaultPermissions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [CaseAgent] =
            [
                AppPermissions.CaseFileCreate,
                AppPermissions.CaseFileView,
                AppPermissions.CaseFileEdit
            ],
            [CaseSupervisor] =
            [
                AppPermissions.CaseFileCreate,
                AppPermissions.CaseFileView,
                AppPermissions.CaseFileEdit,
                AppPermissions.CaseFileClose,
                AppPermissions.CaseFileManage
            ],
            [Auditor] = [AppPermissions.CaseFileView]
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
