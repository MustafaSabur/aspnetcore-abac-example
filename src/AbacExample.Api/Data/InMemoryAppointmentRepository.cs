using AbacExample.Api.Domain;

namespace AbacExample.Api.Data;

public interface IAppointmentRepository
{
    Task<IReadOnlyCollection<Appointment>> ListAsync(CancellationToken cancellationToken = default);

    Task<Appointment?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class InMemoryAppointmentRepository : IAppointmentRepository
{
    private readonly Dictionary<Guid, Appointment> appointments = new()
    {
        [SampleIds.NormalAppointment] = new Appointment
        {
            Id = SampleIds.NormalAppointment,
            TenantId = SampleIds.TenantNorth,
            ClinicId = SampleIds.ClinicNorth,
            PatientId = SampleIds.PatientAlice,
            AssignedClinicianId = SampleIds.ClinicianBen,
            Sensitivity = SensitivityLevel.Normal,
            StartsAt = DateTimeOffset.UtcNow.AddDays(2)
        },
        [SampleIds.SensitiveAppointment] = new Appointment
        {
            Id = SampleIds.SensitiveAppointment,
            TenantId = SampleIds.TenantNorth,
            ClinicId = SampleIds.ClinicNorth,
            PatientId = SampleIds.PatientNora,
            AssignedClinicianId = SampleIds.ClinicianBen,
            Sensitivity = SensitivityLevel.HighlySensitive,
            StartsAt = DateTimeOffset.UtcNow.AddDays(3)
        },
        [SampleIds.OtherTenantAppointment] = new Appointment
        {
            Id = SampleIds.OtherTenantAppointment,
            TenantId = SampleIds.TenantSouth,
            ClinicId = SampleIds.ClinicSouth,
            PatientId = SampleIds.PatientSam,
            AssignedClinicianId = SampleIds.ClinicianDana,
            Sensitivity = SensitivityLevel.Normal,
            StartsAt = DateTimeOffset.UtcNow.AddDays(4)
        }
    };

    public Task<IReadOnlyCollection<Appointment>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Appointment> result;

        lock (appointments)
        {
            result = appointments.Values.ToArray();
        }

        return Task.FromResult(result);
    }

    public Task<Appointment?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (appointments)
        {
            appointments.TryGetValue(id, out var appointment);
            return Task.FromResult(appointment);
        }
    }
}
