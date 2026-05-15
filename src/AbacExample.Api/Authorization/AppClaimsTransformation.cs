using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AbacExample.Api.Authorization;

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
        identity.AddClaim(new Claim(AppClaims.UserKind, ((int)profile.UserKind).ToString()));
        identity.AddClaim(new Claim(AppClaims.Clearance, ((int)profile.Clearance).ToString()));
        identity.AddClaim(new Claim(AppClaims.CanSchedule, BooleanClaimValues.FromBoolean(profile.CanSchedule)));
        identity.AddClaim(new Claim(AppClaims.CanReadClinicAppointments, BooleanClaimValues.FromBoolean(profile.CanReadClinicAppointments)));
        identity.AddClaim(new Claim(AppClaims.CanUsePlatformOverride, BooleanClaimValues.FromBoolean(profile.CanUsePlatformOverride)));
        identity.AddClaim(new Claim(AppClaims.ClaimsVersion, profile.ClaimsVersion.ToString()));

        AddOptional(identity, AppClaims.ClinicId, profile.ClinicId);
        AddOptional(identity, AppClaims.PatientId, profile.PatientId);
        AddOptional(identity, AppClaims.ClinicianId, profile.ClinicianId);

        principal.AddIdentity(identity);
        return principal;
    }

    private static void AddOptional(ClaimsIdentity identity, string claimType, Guid? value)
    {
        if (value is not null)
        {
            identity.AddClaim(new Claim(claimType, value.Value.ToString()));
        }
    }
}
