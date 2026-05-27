namespace AbacExample.Authorization;

public sealed record AuthorizationProfile(
    Guid UserId,
    string ExternalSubjectId,
    Guid TenantId,
    IReadOnlyCollection<string> RoleNames,
    IReadOnlyCollection<string> Permissions,
    bool IsActive,
    long ClaimsVersion);