using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace AbacExample.Authorization;

public sealed class AppClaimsTransformation(
    IAppAuthorizationProfileLoader profileLoader,
    ILogger<AppClaimsTransformation> logger)
    : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true ||
            principal.HasClaim(AppClaims.ProfileLoaded, BooleanClaimValues.True))
        {
            return principal;
        }

        var subject = principal.FindFirstValue(IdentityProviderClaims.Subject);

        if (string.IsNullOrWhiteSpace(subject))
        {
            logger.LogWarning("Authenticated principal has no subject claim.");
            return principal;
        }

        var profile = await profileLoader.LoadBySubjectAsync(subject);

        if (profile is null)
        {
            logger.LogWarning("Authenticated subject has no active app authorization profile.");
            return principal;
        }

        var identity = new ClaimsIdentity("ApplicationAuthorizationProfile");

        identity.AddClaim(new Claim(AppClaims.ProfileLoaded, BooleanClaimValues.True));
        identity.AddClaim(new Claim(AppClaims.UserId, profile.UserId.ToString()));
        identity.AddClaim(new Claim(AppClaims.TenantId, profile.TenantId.ToString()));
        identity.AddClaim(new Claim(AppClaims.ClaimsVersion, profile.ClaimsVersion.ToString()));

        foreach (var roleName in profile.RoleNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(AppClaims.Role, roleName));
        }

        foreach (var permission in profile.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(AppClaims.Permission, permission));
        }

        principal.AddIdentity(identity);
        return principal;
    }
}
