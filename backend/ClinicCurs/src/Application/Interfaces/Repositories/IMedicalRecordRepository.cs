using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IMedicalRecordRepository : IGenericRepository<MedicalRecord>
{
    Task<List<MedicalRecord>> GetPatientHistoryDetailedAsync(Guid accountId, CancellationToken cancellationToken);
}
