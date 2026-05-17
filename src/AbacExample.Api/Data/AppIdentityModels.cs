using AbacExample.Api.Domain;

namespace AbacExample.Api.Data;

public sealed class AppUser
{
    public Guid Id { get; init; }
    public required string ExternalSubjectId { get; init; }
    public Guid TenantId { get; init; }
    public AppUserKind UserKind { get; init; }
    public Guid? ClinicId { get; init; }
    public Guid? PatientId { get; init; }
    public Guid? ClinicianId { get; init; }
    public SensitivityLevel Clearance { get; init; }
    public bool CanUsePlatformOverride { get; init; }
    public bool IsActive { get; init; }
    public long ClaimsVersion { get; init; }
    public ICollection<AppUserRole> Roles { get; } = [];
}

public sealed class AppUserRole
{
    public Guid AppUserId { get; init; }
    public AppUser AppUser { get; init; } = null!;
    public required string RoleName { get; init; }
}
