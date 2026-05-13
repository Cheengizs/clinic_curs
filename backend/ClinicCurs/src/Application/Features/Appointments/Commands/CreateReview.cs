using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Appointments.Commands;

public record CreateReviewResult(bool IsSuccess, string? ErrorMessage);

public record CreateReviewCommand(Guid AccountId, Guid AppointmentId, int Rating, string Comment) : IRequest<CreateReviewResult>;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, CreateReviewResult>
{
    private readonly IGenericRepository<Appointment> _appointmentRepo;
    private readonly IGenericRepository<Review> _reviewRepo;
    private readonly IGenericRepository<Patient> _patientRepo;
    private readonly IGenericRepository<Doctor> _doctorRepo;

    public CreateReviewHandler(
        IGenericRepository<Appointment> appointmentRepo,
        IGenericRepository<Review> reviewRepo,
        IGenericRepository<Patient> patientRepo,
        IGenericRepository<Doctor> doctorRepo)
    {
        _appointmentRepo = appointmentRepo;
        _reviewRepo = reviewRepo;
        _patientRepo = patientRepo;
        _doctorRepo = doctorRepo;
    }

    public async Task<CreateReviewResult> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.Rating < 1 || request.Rating > 5) 
            return new CreateReviewResult(false, "Рейтинг должен быть от 1 до 5 звезд.");

        var appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId);
        if (appointment == null || appointment.AccountId != request.AccountId) 
            return new CreateReviewResult(false, "Прием не найден.");

        if (appointment.Status != AppointmentStatuses.completed)
            return new CreateReviewResult(false, "Оставить отзыв можно только после завершения приема врачом.");

        var existing = await _reviewRepo.FirstOrDefaultAsync(r => r.AppointmentId == request.AppointmentId);
        if (existing != null)
            return new CreateReviewResult(false, "Вы уже оставили отзыв к этому приему.");

        var patient = await _patientRepo.FirstOrDefaultAsync(p => p.AccountId == request.AccountId);
        if (patient == null) return new CreateReviewResult(false, "Профиль пациента не найден.");

        var review = new Review
        {
            DoctorId = appointment.DoctorId.Value,
            PatientId = patient.Id,
            AppointmentId = appointment.Id,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviewRepo.AddAsync(review);
        await _reviewRepo.SaveChangesAsync(); // Сохраняем, чтобы отзыв попал в БД

        // ПЕРЕСЧЕТ СРЕДНЕГО РЕЙТИНГА ВРАЧА
        var allReviews = await _reviewRepo.FindAsync(r => r.DoctorId == appointment.DoctorId.Value);
        var doctor = await _doctorRepo.GetByIdAsync(appointment.DoctorId.Value);
        
        doctor.RatingAvg = (decimal)allReviews.Average(r => r.Rating);
        _doctorRepo.Update(doctor);
        
        await _doctorRepo.SaveChangesAsync();

        return new CreateReviewResult(true, null);
    }
}
