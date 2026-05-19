namespace AbacExample.Api.Data.Entities;

public sealed class AppUser
{
    public Guid Id { get; init; }
    public required string ExternalSubjectId { get; init; }
    public Guid TenantId { get; init; }
    public bool IsActive { get; init; }
    public long ClaimsVersion { get; init; }
    public ICollection<AppUserRole> Roles { get; } = [];
}