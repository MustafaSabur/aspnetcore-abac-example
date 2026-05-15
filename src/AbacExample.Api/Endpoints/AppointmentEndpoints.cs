using System.Security.Claims;
using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Domain;
using Microsoft.AspNetCore.Authorization;

namespace AbacExample.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/appointments")
            .RequireAuthorization()
            .WithTags("Appointments");

        group.MapGet("/{id:guid}", GetAppointment)
            .WithName("GetAppointment");

        group.MapPatch("/{id:guid}/reschedule", RescheduleAppointment)
            .WithName("RescheduleAppointment");

        group.MapPatch("/{id:guid}/clinical-notes", WriteClinicalNotes)
            .WithName("WriteClinicalNotes");

        group.MapPost("/{id:guid}/cancel", CancelAppointment)
            .WithName("CancelAppointment");

        return app;
    }

    private static async Task<IResult> GetAppointment(
        Guid id,
        IAppointmentRepository appointments,
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var appointment = await appointments.FindAsync(id, cancellationToken);

        if (appointment is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(user, appointment, AppointmentOperations.Read);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        return Results.Ok(AppointmentResponse.FromAppointment(appointment));
    }

    private static async Task<IResult> RescheduleAppointment(
        Guid id,
        RescheduleAppointmentRequest request,
        IAppointmentRepository appointments,
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var appointment = await appointments.FindAsync(id, cancellationToken);

        if (appointment is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(user, appointment, AppointmentOperations.Reschedule);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        try
        {
            appointment.Reschedule(request.NewStartTime, timeProvider.GetUtcNow());
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ProblemDetailsResponse(ex.Message));
        }

        return Results.Ok(AppointmentResponse.FromAppointment(appointment));
    }

    private static async Task<IResult> WriteClinicalNotes(
        Guid id,
        WriteClinicalNotesRequest request,
        IAppointmentRepository appointments,
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var appointment = await appointments.FindAsync(id, cancellationToken);

        if (appointment is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(user, appointment, AppointmentOperations.WriteClinicalNotes);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        appointment.WriteClinicalNotes(request.Notes, user.UserId()!.Value, timeProvider.GetUtcNow());
        return Results.Ok(AppointmentResponse.FromAppointment(appointment));
    }

    private static async Task<IResult> CancelAppointment(
        Guid id,
        IAppointmentRepository appointments,
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var appointment = await appointments.FindAsync(id, cancellationToken);

        if (appointment is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(user, appointment, AppointmentOperations.Cancel);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        appointment.Cancel(user.UserId()!.Value, timeProvider.GetUtcNow());
        return Results.Ok(AppointmentResponse.FromAppointment(appointment));
    }
}

public sealed record AppointmentResponse(
    Guid Id,
    Guid TenantId,
    Guid ClinicId,
    Guid PatientId,
    Guid AssignedClinicianId,
    string Status,
    string Sensitivity,
    DateTimeOffset StartsAt,
    string? ClinicalNotes)
{
    public static AppointmentResponse FromAppointment(Appointment appointment) =>
        new(
            appointment.Id,
            appointment.TenantId,
            appointment.ClinicId,
            appointment.PatientId,
            appointment.AssignedClinicianId,
            appointment.Status.ToString(),
            appointment.Sensitivity.ToString(),
            appointment.StartsAt,
            appointment.ClinicalNotes);
}

public sealed record RescheduleAppointmentRequest(DateTimeOffset NewStartTime);

public sealed record WriteClinicalNotesRequest(string Notes);

public sealed record ProblemDetailsResponse(string Error);
