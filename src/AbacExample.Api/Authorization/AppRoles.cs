namespace AbacExample.Api.Authorization;

public static class AppRoles
{
    public const string Admin = "admin";
    public const string Reader = "reader";
    public const string Editor = "editor";

    public static readonly IReadOnlyDictionary<string, string[]> DefaultPermissions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [Admin] =
            [
                AppPermissions.DocumentCreate,
                AppPermissions.DocumentRead,
                AppPermissions.DocumentUpdate,
                AppPermissions.DocumentDelete
            ],
            [Reader] = [AppPermissions.DocumentRead],
            [Editor] =
            [
                AppPermissions.DocumentCreate,
                AppPermissions.DocumentRead,
                AppPermissions.DocumentUpdate
            ]
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
