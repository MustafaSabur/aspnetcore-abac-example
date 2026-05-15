namespace AbacExample.Api.Domain;

public enum AppointmentStatus
{
    Planned = 1,
    Completed = 2,
    Cancelled = 3,
    Locked = 4
}

public enum SensitivityLevel
{
    None = 0,
    Normal = 1,
    Sensitive = 2,
    HighlySensitive = 3
}

public sealed class Appointment
{
    public required Guid Id { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid ClinicId { get; init; }
    public required Guid PatientId { get; init; }
    public required Guid AssignedClinicianId { get; init; }
    public required SensitivityLevel Sensitivity { get; init; }
    public required DateTimeOffset StartsAt { get; set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Planned;
    public string? ClinicalNotes { get; private set; }
    public Guid? ClinicalNotesUpdatedByUserId { get; private set; }
    public DateTimeOffset? ClinicalNotesUpdatedAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    public bool IsLocked =>
        Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.Locked;

    public void Reschedule(DateTimeOffset newStartTime, DateTimeOffset now)
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("Locked appointments cannot be rescheduled.");
        }

        if (newStartTime <= now)
        {
            throw new InvalidOperationException("New appointment time must be in the future.");
        }

        StartsAt = newStartTime;
    }

    public void WriteClinicalNotes(string notes, Guid writtenByUserId, DateTimeOffset now)
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("Locked appointments cannot be edited.");
        }

        ClinicalNotes = notes;
        ClinicalNotesUpdatedByUserId = writtenByUserId;
        ClinicalNotesUpdatedAt = now;
    }

    public void Cancel(Guid cancelledByUserId, DateTimeOffset now)
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("Locked appointments cannot be cancelled.");
        }

        Status = AppointmentStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledAt = now;
    }
}
