using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Profiles.Commands;

public record DeletePatientCommand(Guid TargetAccountId, Guid RequesterAccountId, string RequesterRole) : IRequest<bool>;

public class DeletePatientHandler : IRequestHandler<DeletePatientCommand, bool>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<Patient> _patientRepo;
    private readonly IGenericRepository<Appointment> _appointmentRepo;

    public DeletePatientHandler(
        IGenericRepository<Account> accountRepo,
        IGenericRepository<Patient> patientRepo,
        IGenericRepository<Appointment> appointmentRepo)
    {
        _accountRepo = accountRepo;
        _patientRepo = patientRepo;
        _appointmentRepo = appointmentRepo;
    }

    public async Task<bool> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        // 1. Проверка прав (Пациент может удалить только себя)
        if (request.RequesterRole == "patient" && request.TargetAccountId != request.RequesterAccountId)
            return false;

        var account = await _accountRepo.GetByIdAsync(request.TargetAccountId);
        if (account == null || account.IsDeleted || account.Role != RoleType.patient) return false;

        var patient = await _patientRepo.FirstOrDefaultAsync(p => p.AccountId == account.Id);
        if (patient == null) return false;

        // 2. Анонимизируем данные аккаунта
        account.IsDeleted = true;
        account.Email = $"deleted_{Guid.NewGuid()}@clinic.local"; // Освобождаем Email для новых регистраций
        account.Phone = null;
        account.PasswordHash = "***"; // Блокируем возможность входа
        _accountRepo.Update(account);

        // 3. Анонимизируем медицинский профиль пациента
        patient.FirstName = "Удаленный";
        patient.LastName = "Пользователь";
        patient.MiddleName = "";
        patient.PassportSeriesNumber = "***";
        patient.PersonalNumber = "***";
        patient.ResidentialAddress = "***";
        patient.AvatarUrl = "default_avatar.png";
        patient.BirthDate = new DateOnly(1900, 1, 1);
        _patientRepo.Update(patient);

        // 4. Отменяем все его ПРЕДСТОЯЩИЕ записи к врачам
        var now = DateTime.UtcNow;
        var activeAppointments = await _appointmentRepo.FindAsync(a => 
            a.AccountId == account.Id && 
            a.ScheduledStart > now &&
            (a.Status == AppointmentStatuses.planned || a.Status == AppointmentStatuses.confirmed));

        foreach (var app in activeAppointments)
        {
            app.Status = AppointmentStatuses.cancelled;
            _appointmentRepo.Update(app);
        }

        // Сохраняем все изменения транзакцией
        await _accountRepo.SaveChangesAsync();
        await _patientRepo.SaveChangesAsync();
        await _appointmentRepo.SaveChangesAsync();

        return true;
    }
}
