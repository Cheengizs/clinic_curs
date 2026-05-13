using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class LabRepository : GenericRepository<LabResult>, ILabRepository
{
    public LabRepository(ClinicDbContext context) : base(context) {}

    public async Task<List<LabResult>> GetPatientLabsDetailedAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _context.LabResults
            .Include(r => r.Test)
            .Include(r => r.Office)
            .Where(r => r.MedicalCard.Patient.AccountId == accountId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
