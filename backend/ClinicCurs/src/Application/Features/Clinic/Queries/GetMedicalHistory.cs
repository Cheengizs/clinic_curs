using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Clinic.Queries;

public record MedicalRecordDto(
    Guid Id,
    DateTime Date,
    string DoctorName,
    string Specialization,
    string Complaints,
    string ObjectiveData,
    string Assessment,
    string Plan
);

public record GetMedicalHistoryQuery(Guid AccountId) : IRequest<List<MedicalRecordDto>>;

public class GetMedicalHistoryHandler : IRequestHandler<GetMedicalHistoryQuery, List<MedicalRecordDto>>
{
    private readonly IMedicalRecordRepository _recordRepo;

    public GetMedicalHistoryHandler(IMedicalRecordRepository recordRepo) => _recordRepo = recordRepo;

    public async Task<List<MedicalRecordDto>> Handle(GetMedicalHistoryQuery request, CancellationToken cancellationToken)
    {
        // Вся грязная работа с Include() теперь под капотом репозитория
        var records = await _recordRepo.GetPatientHistoryDetailedAsync(request.AccountId, cancellationToken);

        return records.Select(r => new MedicalRecordDto(
            r.Id,
            r.Appointment.ScheduledStart.ToLocalTime(),
            $"{r.Doctor.LastName} {r.Doctor.FirstName} {r.Doctor.MiddleName}".Trim(),
            r.Doctor.DoctorSpecializations.FirstOrDefault(ds => ds.IsPrimary)?.Specialization.Name ?? "Врач",
            r.Complaints,
            r.ObjectiveData,
            r.Assessment,
            r.Plan
        )).ToList();
    }
}
