namespace AbacExample.Api.Authorization;

public static class AppRoles
{
    public const string Admin = "admin";
    public const string ReadOnly = "read-only";
    public const string Scheduler = "scheduler";
    public const string Clinician = "clinician";

    public static readonly IReadOnlyDictionary<string, string[]> DefaultPermissions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [Admin] =
            [
                AppPermissions.AppointmentCreate,
                AppPermissions.AppointmentRead,
                AppPermissions.AppointmentUpdate,
                AppPermissions.AppointmentDelete
            ],
            [ReadOnly] = [AppPermissions.AppointmentRead],
            [Scheduler] =
            [
                AppPermissions.AppointmentCreate,
                AppPermissions.AppointmentRead,
                AppPermissions.AppointmentUpdate,
                AppPermissions.AppointmentDelete
            ],
            [Clinician] =
            [
                AppPermissions.AppointmentRead,
                AppPermissions.AppointmentUpdate
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
