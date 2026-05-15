using System.Security.Claims;
using AbacExample.Api.Domain;

namespace AbacExample.Api.Authorization;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid TenantId { get; }
    AppUserKind UserKind { get; }
    Guid? ClinicId { get; }
    Guid? ClinicianId { get; }
    Guid? PatientId { get; }
    SensitivityLevel Clearance { get; }
    ClaimsPrincipal Principal { get; }
}