using AbacExample.Authorization;
using AbacExample.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Authorization;

public sealed class DbAuthorizationProfileLoader(AbacExampleDbContext db) : IAuthorizationProfileLoader
{
    public async Task<AuthorizationProfile?> LoadBySubjectAsync(
        string subject,
        CancellationToken cancellationToken = default)
    {
        var user = await db.AuthorizationUsers
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
            .Where(DocumentRoles.IsDefined)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = DocumentRoles.GetPermissionsForRoles(roleNames);

        return new AuthorizationProfile(
            user.Id,
            user.ExternalSubjectId,
            user.TenantId,
            roleNames,
            permissions,
            user.IsActive,
            user.ClaimsVersion);
    }
}
