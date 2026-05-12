using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ClinicDbContext context) : base(context)
    {
    }

    public async Task<(List<Doctor> Doctors, int TotalCount)> GetFilteredDoctorsAsync(
        Guid? officeId, 
        Guid? specializationId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken)
    {
        var query = _context.Doctors
            .Include(d => d.DoctorSpecializations)
            .ThenInclude(ds => ds.Specialization)
            .Where(d => d.IsActive && !d.IsDeleted)
            .AsQueryable();

        if (officeId.HasValue)
        {
            query = query.Where(d => d.OfficeId == officeId.Value);
        }

        if (specializationId.HasValue)
        {
            query = query.Where(d => d.DoctorSpecializations
                .Any(ds => ds.SpecializationId == specializationId.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var doctors = await query
            .OrderByDescending(d => d.RatingAvg)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (doctors, totalCount);
    }
}
