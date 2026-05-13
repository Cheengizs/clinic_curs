using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Labs;

// --- DTOs ---
public record LabTestDto(Guid Id, string Name, string Description);
public record PatientLabResultDto(Guid Id, string TestName, string Date, string OfficeName, string FileUrl);

// --- 1. ПОЛУЧЕНИЕ СПРАВОЧНИКА АНАЛИЗОВ ---
public record GetLabTestsQuery() : IRequest<List<LabTestDto>>;
public class GetLabTestsHandler : IRequestHandler<GetLabTestsQuery, List<LabTestDto>>
{
    private readonly IGenericRepository<LabTestsDictionary> _testRepo;
    public GetLabTestsHandler(IGenericRepository<LabTestsDictionary> testRepo) => _testRepo = testRepo;

    public async Task<List<LabTestDto>> Handle(GetLabTestsQuery request, CancellationToken ct)
    {
        var tests = await _testRepo.GetAllAsync();
        return tests.Select(t => new LabTestDto(t.Id, t.Name, t.Description)).OrderBy(t => t.Name).ToList();
    }
}

// --- 2. ПОЛУЧЕНИЕ АНАЛИЗОВ ПАЦИЕНТОМ ---
public record GetMyLabResultsQuery(Guid AccountId) : IRequest<List<PatientLabResultDto>>;
public class GetMyLabResultsHandler : IRequestHandler<GetMyLabResultsQuery, List<PatientLabResultDto>>
{
    private readonly ILabRepository _labRepo;
    public GetMyLabResultsHandler(ILabRepository labRepo) => _labRepo = labRepo;

    public async Task<List<PatientLabResultDto>> Handle(GetMyLabResultsQuery request, CancellationToken ct)
    {
        var results = await _labRepo.GetPatientLabsDetailedAsync(request.AccountId, ct);
        return results.Select(r => new PatientLabResultDto(r.Id, r.Test.Name, r.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"), r.Office.Name, r.ResultFileUrl)).ToList();
    }
}

// --- 3. ЗАГРУЗКА АНАЛИЗА ПЕРСОНАЛОМ ---
public record AddLabResultCommand(Guid StaffAccountId, Guid PatientAccountId, Guid TestId, string FileId) : IRequest<bool>;
public class AddLabResultHandler : IRequestHandler<AddLabResultCommand, bool>
{
    private readonly IGenericRepository<Registrar> _regRepo;
    private readonly IGenericRepository<Doctor> _docRepo;
    private readonly IGenericRepository<Patient> _patRepo;
    private readonly IGenericRepository<MedicalCard> _cardRepo;
    private readonly ILabRepository _labRepo;

    public AddLabResultHandler(IGenericRepository<Registrar> regRepo, IGenericRepository<Doctor> docRepo, IGenericRepository<Patient> patRepo, IGenericRepository<MedicalCard> cardRepo, ILabRepository labRepo)
    {
        _regRepo = regRepo; _docRepo = docRepo; _patRepo = patRepo; _cardRepo = cardRepo; _labRepo = labRepo;
    }

    public async Task<bool> Handle(AddLabResultCommand request, CancellationToken ct)
    {
        // Узнаем офис, в котором работает сотрудник (Врач или Регистратор)
        var registrar = await _regRepo.FirstOrDefaultAsync(r => r.AccountId == request.StaffAccountId);
        var doctor = await _docRepo.FirstOrDefaultAsync(d => d.AccountId == request.StaffAccountId);
        var officeId = registrar?.OfficeId ?? doctor?.OfficeId;
        if (officeId == null) return false;

        // Ищем пациента и его карту
        var patient = await _patRepo.FirstOrDefaultAsync(p => p.AccountId == request.PatientAccountId);
        if (patient == null) return false;
        var card = await _cardRepo.FirstOrDefaultAsync(c => c.PatientId == patient.Id);
        if (card == null) return false;

        var labResult = new LabResult
        {
            CardId = card.Id,
            MedicalCard = card, // <-- ИЗБЕГАЕМ БАГА EF CORE
            TestId = request.TestId,
            OfficeId = officeId.Value,
            ResultFileUrl = request.FileId,
            Status = LabStatus.ready,
            CreatedAt = DateTime.UtcNow
        };

        await _labRepo.AddAsync(labResult);
        await _labRepo.SaveChangesAsync();
        return true;
    }
}
