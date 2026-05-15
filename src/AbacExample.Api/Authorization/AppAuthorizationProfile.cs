using AbacExample.Api.Domain;

namespace AbacExample.Api.Authorization;

public sealed record AppAuthorizationProfile(
    Guid UserId,
    string ExternalSubjectId,
    Guid TenantId,
    AppUserKind UserKind,
    Guid? ClinicId,
    Guid? PatientId,
    Guid? ClinicianId,
    SensitivityLevel Clearance,
    bool CanSchedule,
    bool CanReadClinicAppointments,
    bool CanUsePlatformOverride,
    bool IsActive,
    long ClaimsVersion);

public interface IAppAuthorizationProfileLoader
{
    Task<AppAuthorizationProfile?> LoadBySubjectAsync(
        string subject,
        CancellationToken cancellationToken = default);
}
