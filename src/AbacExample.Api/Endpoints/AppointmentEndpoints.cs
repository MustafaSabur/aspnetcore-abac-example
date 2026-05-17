using System.Security.Claims;
using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/appointments")
            .RequireAuthorization()
            .WithTags("Appointments");

        group.MapPost("/", CreateAppointment)
            .RequireAuthorization(AppPermissions.AppointmentCreate)
            .WithName("CreateAppointment");

        group.MapGet("/{id:guid}", GetAppointment)
            .RequireAuthorization(AppPermissions.AppointmentRead)
            .WithName("GetAppointment");

        group.MapGet("/{id:guid}/edit-context", GetAppointment)
            .RequireAnyPermission(
                AppPermissions.AppointmentRead,
                AppPermissions.AppointmentUpdate)
            .WithName("GetAppointmentEditContext");

        group.MapPatch("/{id:guid}/reschedule", RescheduleAppointment)
            .RequireAuthorization(AppPermissions.AppointmentUpdate)
            .WithName("RescheduleAppointment");

        group.MapPatch("/{id:guid}/clinical-notes", WriteClinicalNotes)
            .RequireAuthorization(AppPermissions.AppointmentUpdate)
            .WithName("WriteClinicalNotes");

        group.MapPost("/{id:guid}/cancel", CancelAppointment)
            .RequireAuthorization(AppPermissions.AppointmentDelete)
            .WithName("CancelAppointment");

        return app;
    }

    private static async Task<IResult> CreateAppointment(
        CreateAppointmentRequest request,
        AppDbContext db,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = user.TenantId()!.Value,
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            AssignedClinicianId = request.AssignedClinicianId,
            Sensitivity = request.Sensitivity,
            StartsAt = request.StartsAt
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/appointments/{appointment.Id}", AppointmentResponse.FromAppointment(appointment));
    }

    private static async Task<IResult> GetAppointment(
        Guid id,
        AppDbContext db,
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

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
        AppDbContext db,
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

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

        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(AppointmentResponse.FromAppointment(appointment));
    }

    private static async Task<IResult> WriteClinicalNotes(
        Guid id,
        WriteClinicalNotesRequest request,
        AppDbContext db,
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

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
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(AppointmentResponse.FromAppointment(appointment));
    }

    private static async Task<IResult> CancelAppointment(
        Guid id,
        AppDbContext db,
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

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
        await db.SaveChangesAsync(cancellationToken);

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

public sealed record CreateAppointmentRequest(
    Guid ClinicId,
    Guid PatientId,
    Guid AssignedClinicianId,
    SensitivityLevel Sensitivity,
    DateTimeOffset StartsAt);

public sealed record RescheduleAppointmentRequest(DateTimeOffset NewStartTime);

public sealed record WriteClinicalNotesRequest(string Notes);

public sealed record ProblemDetailsResponse(string Error);
