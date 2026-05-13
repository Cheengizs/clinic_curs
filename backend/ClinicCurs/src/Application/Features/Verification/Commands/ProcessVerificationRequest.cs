using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Verification.Commands;

public record ProcessVerificationResult(bool IsSuccess, string? ErrorMessage);

public record ProcessVerificationRequestCommand(
    Guid RequestId, 
    Guid RegistrarAccountId, 
    bool IsApproved) : IRequest<ProcessVerificationResult>;

public class ProcessVerificationRequestHandler : IRequestHandler<ProcessVerificationRequestCommand, ProcessVerificationResult>
{
    private readonly IGenericRepository<VerificationRequest> _requestRepo;
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<Registrar> _registrarRepo;
    private readonly IGenericRepository<Patient> _patientRepo;
    private readonly IGenericRepository<MedicalCard> _medicalCardRepo;

    public ProcessVerificationRequestHandler(
        IGenericRepository<VerificationRequest> requestRepo,
        IGenericRepository<Account> accountRepo,
        IGenericRepository<Registrar> registrarRepo,
        IGenericRepository<Patient> patientRepo,
        IGenericRepository<MedicalCard> medicalCardRepo)
    {
        _requestRepo = requestRepo;
        _accountRepo = accountRepo;
        _registrarRepo = registrarRepo;
        _patientRepo = patientRepo;
        _medicalCardRepo = medicalCardRepo;
    }

    public async Task<ProcessVerificationResult> Handle(ProcessVerificationRequestCommand request, CancellationToken cancellationToken)
    {
        var verificationReq = await _requestRepo.GetByIdAsync(request.RequestId);
        if (verificationReq == null || verificationReq.Status != VerificationStatuses.wait)
            return new ProcessVerificationResult(false, "Заявка не найдена или уже была обработана.");

        var registrar = await _registrarRepo.FirstOrDefaultAsync(r => r.AccountId == request.RegistrarAccountId);
        if (registrar == null)
            return new ProcessVerificationResult(false, "Профиль регистратора не найден.");

        verificationReq.RegistrarId = registrar.Id;
        verificationReq.ProcessedAt = DateTime.UtcNow;
        verificationReq.Status = request.IsApproved ? VerificationStatuses.verified : VerificationStatuses.declined;
        
        _requestRepo.Update(verificationReq);

        if (request.IsApproved)
        {
            var account = await _accountRepo.GetByIdAsync(verificationReq.AccountId);
            if (account != null)
            {
                account.IdentityVerified = true;
                _accountRepo.Update(account);
            }

            var patient = new Patient
            {
                AccountId = verificationReq.AccountId,
                FirstName = verificationReq.FirstName,
                LastName = verificationReq.LastName,
                MiddleName = verificationReq.MiddleName,
                BirthDate = verificationReq.BirthDate,
                
                Gender = verificationReq.Gender, // <--- БЕРЕМ РЕАЛЬНЫЕ ДАННЫЕ
                ResidentialAddress = verificationReq.ResidentialAddress, // <--- БЕРЕМ РЕАЛЬНЫЕ ДАННЫЕ
                
                PassportSeriesNumber = verificationReq.PassportSeriesNumber,
                PersonalNumber = verificationReq.PersonalNumber,
                AvatarUrl = "default_avatar.png"
            };

            await _patientRepo.AddAsync(patient);
            await _patientRepo.SaveChangesAsync();

            var medicalCard = new MedicalCard
            {
                PatientId = patient.Id,
                CardNumber = $"MC-{DateTime.UtcNow:yyyyMMdd}-{patient.PersonalNumber.Substring(Math.Max(0, patient.PersonalNumber.Length - 4))}",
                BloodType = BloodTypeEnum.O_first, 
                RhesusFactor = RhesusFactorEnum.positive,
                ChronicDiseases = "Нет данных",
                Allergies = "Нет данных"
            };

            await _medicalCardRepo.AddAsync(medicalCard);
        }

        await _requestRepo.SaveChangesAsync();
        await _medicalCardRepo.SaveChangesAsync();

        return new ProcessVerificationResult(true, null);
    }
}
