using System.Security.Claims;

namespace AbacExample.Authorization;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal user)
    {
        public Guid? UserId() => user.GetGuidClaim(AppClaims.UserId);
        public Guid? TenantId() => user.GetGuidClaim(AppClaims.TenantId);

        public bool HasPermission(string permission) =>
            user.Claims.Any(claim =>
                claim.Type == AppClaims.Permission &&
                string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));

        public bool HasMfa() =>
            user.Claims.Any(claim =>
                claim.Type == IdentityProviderClaims.AuthenticationMethod &&
                string.Equals(claim.Value, IdentityProviderClaims.Mfa, StringComparison.OrdinalIgnoreCase));

        public Guid? GetGuidClaim(string claimType)
        {
            var value = user.FindFirstValue(claimType);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
