namespace AbacExample.Api.Authorization;

public static class IdentityProviderClaims
{
    public const string Subject = "sub";
    public const string AuthenticationMethod = "amr";
    public const string Mfa = "mfa";
}

public static class AppClaims
{
    public const string ProfileLoaded = "app_profile_loaded";
    public const string UserId = "app_user_id";
    public const string TenantId = "tenant_id";
    public const string Role = "app_role";
    public const string Permission = "app_permission";
    public const string ClaimsVersion = "claims_version";
}

public static class BooleanClaimValues
{
    public const string True = "true";
    public const string False = "false";

    public static string FromBoolean(bool value) => value ? True : False;
}
