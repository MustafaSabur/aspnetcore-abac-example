using System.Security.Claims;
using AbacExample.Api.Authorization;
using AbacExample.Api.Data;

namespace AbacExample.Api.Endpoints;

public static class SampleEndpoints
{
    public static IEndpointRouteBuilder MapSampleEndpoints(
        this IEndpointRouteBuilder app,
        bool sampleAuthenticationEnabled)
    {
        var group = app.MapGroup("/sample")
            .WithTags("Sample data");

        group.MapGet("/users", () =>
        {
            var users = InMemoryAppAuthorizationProfileLoader.SampleProfiles
                .Select(profile => new SampleUserResponse(
                    profile.ExternalSubjectId,
                    profile.UserKind.ToString(),
                    profile.TenantId,
                    profile.ClinicId,
                    profile.PatientId,
                    profile.ClinicianId,
                    profile.Clearance.ToString(),
                    profile.CanSchedule,
                    profile.CanReadClinicAppointments,
                    profile.CanUsePlatformOverride))
                .ToArray();

            return Results.Ok(new
            {
                sampleAuthenticationEnabled,
                subjectHeader = "X-Sample-Subject",
                mfaHeader = "X-Sample-Mfa: true",
                users
            });
        }).AllowAnonymous();

        group.MapGet("/appointments", async (IAppointmentRepository appointments) =>
        {
            var result = (await appointments.ListAsync())
                .Select(AppointmentResponse.FromAppointment)
                .ToArray();

            return Results.Ok(result);
        }).AllowAnonymous();

        group.MapGet("/whoami", (ClaimsPrincipal user) =>
        {
            var response = new CurrentUserResponse(
                user.FindFirstValue(IdentityProviderClaims.Subject),
                user.UserId(),
                user.TenantId(),
                user.UserKind()?.ToString(),
                user.ClinicId(),
                user.PatientId(),
                user.ClinicianId(),
                user.Clearance().ToString(),
                user.CanSchedule(),
                user.CanReadClinicAppointments(),
                user.CanUsePlatformOverride(),
                user.HasMfa());

            return Results.Ok(response);
        });

        return app;
    }
}

public sealed record SampleUserResponse(
    string Subject,
    string UserKind,
    Guid TenantId,
    Guid? ClinicId,
    Guid? PatientId,
    Guid? ClinicianId,
    string Clearance,
    bool CanSchedule,
    bool CanReadClinicAppointments,
    bool CanUsePlatformOverride);

public sealed record CurrentUserResponse(
    string? Subject,
    Guid? UserId,
    Guid? TenantId,
    string? UserKind,
    Guid? ClinicId,
    Guid? PatientId,
    Guid? ClinicianId,
    string Clearance,
    bool CanSchedule,
    bool CanReadClinicAppointments,
    bool CanUsePlatformOverride,
    bool HasMfa);
