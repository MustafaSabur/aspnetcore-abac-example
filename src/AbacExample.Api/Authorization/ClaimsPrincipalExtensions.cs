using System.Security.Claims;
using AbacExample.Api.Domain;

namespace AbacExample.Api.Authorization;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal user)
    {
        public Guid? UserId() => user.GetGuidClaim(AppClaims.UserId);
        public Guid? TenantId() => user.GetGuidClaim(AppClaims.TenantId);
        public Guid? ClinicId() => user.GetGuidClaim(AppClaims.ClinicId);
        public Guid? PatientId() => user.GetGuidClaim(AppClaims.PatientId);
        public Guid? ClinicianId() => user.GetGuidClaim(AppClaims.ClinicianId);
        public bool CanUsePlatformOverride() =>
            user.HasBooleanClaim(AppClaims.CanUsePlatformOverride);

        public bool HasPermission(string permission) =>
            user.Claims.Any(claim =>
                claim.Type == AppClaims.Permission &&
                string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));

        public AppUserKind? UserKind()
        {
            var value = user.GetIntClaimOrDefault(AppClaims.UserKind);
            return Enum.IsDefined(typeof(AppUserKind), value) ? (AppUserKind)value : null;
        }

        public SensitivityLevel Clearance()
        {
            var value = user.GetIntClaimOrDefault(AppClaims.Clearance);
            return Enum.IsDefined(typeof(SensitivityLevel), value) ? (SensitivityLevel)value : SensitivityLevel.None;
        }

        public bool HasMfa()
        {
            return user.Claims.Any(claim =>
                claim.Type == IdentityProviderClaims.AuthenticationMethod &&
                string.Equals(claim.Value, IdentityProviderClaims.Mfa, StringComparison.OrdinalIgnoreCase));
        }

        public Guid? GetGuidClaim(string claimType)
        {
            var value = user.FindFirstValue(claimType);
            return Guid.TryParse(value, out var id) ? id : null;
        }

        public bool HasBooleanClaim(string claimType)
        {
            var value = user.FindFirstValue(claimType);
            return bool.TryParse(value, out var parsed) && parsed;
        }

        public int GetIntClaimOrDefault(string claimType, int defaultValue = 0)
        {
            var value = user.FindFirstValue(claimType);
            return int.TryParse(value, out var parsed) ? parsed : defaultValue;
        }
    }
}
