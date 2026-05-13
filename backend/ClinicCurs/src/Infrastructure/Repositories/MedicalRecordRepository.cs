using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MedicalRecordRepository : GenericRepository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(ClinicDbContext context) : base(context)
    {
    }

    public async Task<List<MedicalRecord>> GetPatientHistoryDetailedAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _context.MedicalRecords
            .Include(r => r.MedicalCard)
            .ThenInclude(mc => mc.Patient)
            .Include(r => r.Doctor)
            .ThenInclude(d => d.DoctorSpecializations)
            .ThenInclude(ds => ds.Specialization)
            .Include(r => r.Appointment)
            .Where(r => r.MedicalCard.Patient.AccountId == accountId)
            .OrderByDescending(r => r.Appointment.ScheduledStart)
            .ToListAsync(cancellationToken);
    }
}
