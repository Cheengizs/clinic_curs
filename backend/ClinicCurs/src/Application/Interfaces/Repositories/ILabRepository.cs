using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface ILabRepository : IGenericRepository<LabResult>
{
    Task<List<LabResult>> GetPatientLabsDetailedAsync(Guid accountId, CancellationToken cancellationToken);
}
