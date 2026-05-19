using AbacExample.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Authorization;

public sealed class DbAppAuthorizationProfileLoader(AppDbContext db) : IAppAuthorizationProfileLoader
{
    public async Task<AppAuthorizationProfile?> LoadBySubjectAsync(
        string subject,
        CancellationToken cancellationToken = default)
    {
        var user = await db.AppUsers
            .AsNoTracking()
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(
                x => x.ExternalSubjectId == subject && x.IsActive,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleNames = user.Roles
            .Select(x => x.RoleName)
            .Where(AppRoles.IsDefined)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = AppRoles.GetPermissionsForRoles(roleNames);

        return new AppAuthorizationProfile(
            user.Id,
            user.ExternalSubjectId,
            user.TenantId,
            roleNames,
            permissions,
            user.IsActive,
            user.ClaimsVersion);
    }
}
