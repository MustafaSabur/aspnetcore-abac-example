using AbacExample.Api.Domain;

namespace AbacExample.Api.Data;

public interface IAppointmentRepository
{
    Task<Appointment?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}
