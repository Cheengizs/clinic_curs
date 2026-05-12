using Application.Interfaces.Repositories;
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

        // 2. Ищем профиль регистратора (он должен существовать, чтобы привязать обработку)
        var registrar = await _registrarRepo.FirstOrDefaultAsync(r => r.AccountId == request.RegistrarAccountId);
        if (registrar == null)
            return new ProcessVerificationResult(false, "Профиль регистратора не найден. Обратитесь к администратору.");

        // Обновляем саму заявку
        verificationReq.RegistrarId = registrar.Id;
        verificationReq.ProcessedAt = DateTime.UtcNow;
        verificationReq.Status = request.IsApproved ? VerificationStatuses.verified : VerificationStatuses.declined;
        
        _requestRepo.Update(verificationReq);

        // 3. Если одобрено — создаем пациента и медкарту
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
                PassportSeriesNumber = verificationReq.PassportSeriesNumber,
                PersonalNumber = verificationReq.PersonalNumber,
                Gender = Gender.male, // Default. Потом можно добавить выбор пола в заявку
                ResidentialAddress = "Не указан", 
                AvatarUrl = "default_avatar.png"
            };

            await _patientRepo.AddAsync(patient);
            
            // Сохраняем, чтобы БД сгенерировала Patient.Id для медкарты
            await _patientRepo.SaveChangesAsync();

            var medicalCard = new MedicalCard
            {
                PatientId = patient.Id,
                // Генерируем красивый номер карты: MC-ГодМесяцДень-Последние4ЦифрыИНН
                CardNumber = $"MC-{DateTime.UtcNow:yyyyMMdd}-{patient.PersonalNumber.Substring(Math.Max(0, patient.PersonalNumber.Length - 4))}",
                BloodType = BloodTypeEnum.O_first, // Default, пока не сдадут кровь
                RhesusFactor = RhesusFactorEnum.positive,
                ChronicDiseases = "Нет данных",
                Allergies = "Нет данных"
            };

            await _medicalCardRepo.AddAsync(medicalCard);
        }

        // 4. Финальное сохранение всех изменений
        await _requestRepo.SaveChangesAsync();
        await _medicalCardRepo.SaveChangesAsync();

        return new ProcessVerificationResult(true, null);
    }
}
