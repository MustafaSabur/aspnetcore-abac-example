using System.Security.Claims;
using AbacExample.Api.Domain;

namespace AbacExample.Api.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static Guid? UserId(this ClaimsPrincipal user) => user.GetGuidClaim(AppClaims.UserId);

    public static Guid? TenantId(this ClaimsPrincipal user) => user.GetGuidClaim(AppClaims.TenantId);

    public static Guid? ClinicId(this ClaimsPrincipal user) => user.GetGuidClaim(AppClaims.ClinicId);

    public static Guid? PatientId(this ClaimsPrincipal user) => user.GetGuidClaim(AppClaims.PatientId);

    public static Guid? ClinicianId(this ClaimsPrincipal user) => user.GetGuidClaim(AppClaims.ClinicianId);

    public static bool CanSchedule(this ClaimsPrincipal user) => user.HasBooleanClaim(AppClaims.CanSchedule);

    public static bool CanReadClinicAppointments(this ClaimsPrincipal user) =>
        user.HasBooleanClaim(AppClaims.CanReadClinicAppointments);

    public static bool CanUsePlatformOverride(this ClaimsPrincipal user) =>
        user.HasBooleanClaim(AppClaims.CanUsePlatformOverride);

    public static AppUserKind? UserKind(this ClaimsPrincipal user)
    {
        var value = user.GetIntClaimOrDefault(AppClaims.UserKind);
        return Enum.IsDefined(typeof(AppUserKind), value) ? (AppUserKind)value : null;
    }

    public static SensitivityLevel Clearance(this ClaimsPrincipal user)
    {
        var value = user.GetIntClaimOrDefault(AppClaims.Clearance);
        return Enum.IsDefined(typeof(SensitivityLevel), value) ? (SensitivityLevel)value : SensitivityLevel.None;
    }

    public static bool HasMfa(this ClaimsPrincipal user)
    {
        return user.Claims.Any(claim =>
            claim.Type == IdentityProviderClaims.AuthenticationMethod &&
            string.Equals(claim.Value, IdentityProviderClaims.Mfa, StringComparison.OrdinalIgnoreCase));
    }

    public static Guid? GetGuidClaim(this ClaimsPrincipal user, string claimType)
    {
        var value = user.FindFirstValue(claimType);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static bool HasBooleanClaim(this ClaimsPrincipal user, string claimType)
    {
        var value = user.FindFirstValue(claimType);
        return bool.TryParse(value, out var parsed) && parsed;
    }

    public static int GetIntClaimOrDefault(this ClaimsPrincipal user, string claimType, int defaultValue = 0)
    {
        var value = user.FindFirstValue(claimType);
        return int.TryParse(value, out var parsed) ? parsed : defaultValue;
    }
}
