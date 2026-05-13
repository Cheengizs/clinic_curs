using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ClinicDbContext context) : base(context) {}

    public async Task<List<Appointment>> GetPatientAppointmentsDetailedAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .ThenInclude(d => d.DoctorSpecializations)
            .ThenInclude(ds => ds.Specialization)
            .Include(a => a.Office)
            .Include(a => a.Type)
            .Include(a => a.Review) // <--- ДОБАВИЛИ ПОДГРУЗКУ ОТЗЫВОВ
            .Where(a => a.AccountId == accountId)
            .OrderByDescending(a => a.ScheduledStart)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<Appointment>> GetDoctorAppointmentsDetailedAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken)
    {
        var startOfDay = date.Date.ToUniversalTime();
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Appointments
            .Include(a => a.Account)
            .ThenInclude(ac => ac.Patient) // Подтягиваем профиль пациента
            .Include(a => a.Type)              // Подтягиваем услугу
            .Where(a => a.DoctorId == doctorId && a.ScheduledStart >= startOfDay && a.ScheduledStart < endOfDay)
            .OrderBy(a => a.ScheduledStart)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<Appointment>> GetOfficeAppointmentsDetailedAsync(Guid officeId, DateTime date, CancellationToken cancellationToken)
    {
        var start = date.Date.ToUniversalTime();
        var end = start.AddDays(1);
        
        return await _context.Appointments
            .Include(a => a.Account).ThenInclude(ac => ac.Patient) // Пациент
            .Include(a => a.Doctor)                               // Врач
            .Include(a => a.Type)
            .Where(a => a.OfficeId == officeId && a.ScheduledStart >= start && a.ScheduledStart < end)
            .OrderBy(a => a.ScheduledStart)
            .ToListAsync(cancellationToken);
    }
}
