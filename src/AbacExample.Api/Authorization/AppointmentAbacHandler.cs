using System.Security.Claims;
using AbacExample.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace AbacExample.Api.Authorization;

public sealed class AppointmentAbacHandler(
    TimeProvider timeProvider,
    ILogger<AppointmentAbacHandler> logger)
    : AuthorizationHandler<OperationAuthorizationRequirement, Appointment>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Appointment appointment)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true ||
            !user.HasClaim(AppClaims.ProfileLoaded, BooleanClaimValues.True))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (CanUsePlatformOverride(user))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (user.TenantId() != appointment.TenantId)
        {
            logger.LogWarning(
                "ABAC hard denied tenant mismatch. UserId={UserId} AppointmentId={AppointmentId}",
                user.UserId(),
                appointment.Id);

            context.Fail();
            return Task.CompletedTask;
        }

        var now = timeProvider.GetUtcNow();

        var allowed = requirement.Name switch
        {
            nameof(AppointmentOperations.Read) => CanRead(user, appointment),
            nameof(AppointmentOperations.Reschedule) => CanReschedule(user, appointment, now),
            nameof(AppointmentOperations.WriteClinicalNotes) => CanWriteClinicalNotes(user, appointment),
            nameof(AppointmentOperations.Cancel) => CanCancel(user, appointment, now),
            _ => false
        };

        if (allowed)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool CanRead(ClaimsPrincipal user, Appointment appointment)
    {
        if (user.PatientId() == appointment.PatientId)
        {
            return appointment.Sensitivity == SensitivityLevel.Normal;
        }

        if (user.ClinicianId() == appointment.AssignedClinicianId)
        {
            return HasEnoughClearance(user, appointment);
        }

        return user.ClinicId() == appointment.ClinicId
               && user.CanReadClinicAppointments()
               && HasEnoughClearance(user, appointment);
    }

    private static bool CanReschedule(ClaimsPrincipal user, Appointment appointment, DateTimeOffset now) =>
        user.ClinicId() == appointment.ClinicId
        && user.CanSchedule()
        && !appointment.IsLocked
        && appointment.StartsAt > now;

    private static bool CanWriteClinicalNotes(ClaimsPrincipal user, Appointment appointment) =>
        user.ClinicianId() == appointment.AssignedClinicianId
        && !appointment.IsLocked
        && HasEnoughClearance(user, appointment)
        && (appointment.Sensitivity != SensitivityLevel.HighlySensitive || user.HasMfa());

    private static bool CanCancel(ClaimsPrincipal user, Appointment appointment, DateTimeOffset now) =>
        user.ClinicId() == appointment.ClinicId
        && user.CanSchedule()
        && !appointment.IsLocked
        && appointment.StartsAt > now;

    private static bool CanUsePlatformOverride(ClaimsPrincipal user) =>
        user.UserKind() == AppUserKind.PlatformAdmin
        && user.CanUsePlatformOverride()
        && user.HasMfa();

    private static bool HasEnoughClearance(ClaimsPrincipal user, Appointment appointment) =>
        user.Clearance() >= appointment.Sensitivity;
}
