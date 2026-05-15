using System.Security.Claims;
using AbacExample.Api.Domain;

namespace AbacExample.Api.Authorization;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public ClaimsPrincipal Principal =>
        httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No active HTTP context.");

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public Guid UserId =>
        Principal.UserId() ?? throw new InvalidOperationException("Missing app user id claim.");

    public Guid TenantId =>
        Principal.TenantId() ?? throw new InvalidOperationException("Missing tenant id claim.");

    public AppUserKind UserKind =>
        Principal.UserKind() ?? throw new InvalidOperationException("Missing user kind claim.");

    public Guid? ClinicId => Principal.ClinicId();

    public Guid? ClinicianId => Principal.ClinicianId();

    public Guid? PatientId => Principal.PatientId();

    public SensitivityLevel Clearance => Principal.Clearance();
}
