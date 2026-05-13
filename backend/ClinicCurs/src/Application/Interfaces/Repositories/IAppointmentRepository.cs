using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<List<Appointment>> GetPatientAppointmentsDetailedAsync(Guid accountId, CancellationToken cancellationToken);

    Task<List<Appointment>> GetDoctorAppointmentsDetailedAsync(Guid doctorId, DateTime date,
        CancellationToken cancellationToken);
    Task<List<Appointment>> GetOfficeAppointmentsDetailedAsync(Guid officeId, DateTime date, CancellationToken cancellationToken);
}
