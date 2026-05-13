using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Appointments.Commands;

public record CompleteAppointmentResult(bool IsSuccess, string? ErrorMessage);

public record CompleteAppointmentCommand(
    Guid DoctorAccountId,
    Guid AppointmentId,
    string Complaints,
    string ObjectiveData,
    string Assessment,
    string Plan
) : IRequest<CompleteAppointmentResult>;

public class CompleteAppointmentHandler : IRequestHandler<CompleteAppointmentCommand, CompleteAppointmentResult>
{
    private readonly IGenericRepository<Appointment> _appointmentRepo;
    private readonly IGenericRepository<Doctor> _doctorRepo;
    private readonly IGenericRepository<Patient> _patientRepo;
    private readonly IGenericRepository<MedicalCard> _cardRepo;
    private readonly IGenericRepository<MedicalRecord> _recordRepo;

    public CompleteAppointmentHandler(
        IGenericRepository<Appointment> appointmentRepo,
        IGenericRepository<Doctor> doctorRepo,
        IGenericRepository<Patient> patientRepo,
        IGenericRepository<MedicalCard> cardRepo,
        IGenericRepository<MedicalRecord> recordRepo)
    {
        _appointmentRepo = appointmentRepo;
        _doctorRepo = doctorRepo;
        _patientRepo = patientRepo;
        _cardRepo = cardRepo;
        _recordRepo = recordRepo;
    }

    public async Task<CompleteAppointmentResult> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Ищем врача
        var doctor = await _doctorRepo.FirstOrDefaultAsync(d => d.AccountId == request.DoctorAccountId);
        if (doctor == null) return new CompleteAppointmentResult(false, "Врач не найден");

        // 2. Ищем запись
        var appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId);
        if (appointment == null || appointment.DoctorId != doctor.Id)
            return new CompleteAppointmentResult(false, "Запись не найдена или недоступна");

        if (appointment.Status == AppointmentStatuses.completed)
            return new CompleteAppointmentResult(false, "Этот прием уже был завершен");

        // 3. Ищем пациента и его медкарту
        var patient = await _patientRepo.FirstOrDefaultAsync(p => p.AccountId == appointment.AccountId);
        if (patient == null) return new CompleteAppointmentResult(false, "Профиль пациента не найден");

        var card = await _cardRepo.FirstOrDefaultAsync(c => c.PatientId == patient.Id);
        if (card == null) return new CompleteAppointmentResult(false, "Медкарта пациента не найдена (ошибка верификации)");

        // 4. Создаем запись в медкарте
        var record = new MedicalRecord
        {
            CardId = card.Id,
            AppointmentId = appointment.Id,
            MedicalCard = card,
            DoctorId = doctor.Id,
            Complaints = request.Complaints,
            ObjectiveData = request.ObjectiveData,
            Assessment = request.Assessment,
            Plan = request.Plan
        };

        await _recordRepo.AddAsync(record);
        
        // 5. Меняем статус приема
        appointment.Status = AppointmentStatuses.completed;
        _appointmentRepo.Update(appointment);

        // Сохраняем все транзакции
        await _recordRepo.SaveChangesAsync();

        return new CompleteAppointmentResult(true, null);
    }
}
