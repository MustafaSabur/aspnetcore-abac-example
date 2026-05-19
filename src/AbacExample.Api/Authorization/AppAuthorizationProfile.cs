namespace AbacExample.Api.Authorization;

public sealed record AppAuthorizationProfile(
    Guid UserId,
    string ExternalSubjectId,
    Guid TenantId,
    IReadOnlyCollection<string> RoleNames,
    IReadOnlyCollection<string> Permissions,
    bool IsActive,
    long ClaimsVersion);