using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<(List<Doctor> Doctors, int TotalCount)> GetFilteredDoctorsAsync(
        Guid? officeId, 
        Guid? specializationId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken);
}
