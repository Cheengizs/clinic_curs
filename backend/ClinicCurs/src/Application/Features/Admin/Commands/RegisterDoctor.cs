using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Features.Admin.Commands;

public record RegisterDoctorResult(bool IsSuccess, string? ErrorMessage, string? Email, string? Password);

public record SpecializationInput(Guid SpecializationId, bool IsPrimary, DateOnly? CareerStartDate);

public record RegisterDoctorCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Phone,
    Guid OfficeId,
    string Bio,
    List<SpecializationInput> Specializations // Массив специализаций
) : IRequest<RegisterDoctorResult>;

public class RegisterDoctorHandler : IRequestHandler<RegisterDoctorCommand, RegisterDoctorResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<Doctor> _doctorRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IEmailService _emailService;

    public RegisterDoctorHandler(
        IGenericRepository<Account> accountRepo, 
        IGenericRepository<Doctor> doctorRepo, 
        IPasswordHasher hasher,
        IEmailService emailService)
    {
        _accountRepo = accountRepo;
        _doctorRepo = doctorRepo;
        _hasher = hasher;
        _emailService = emailService;
    }

    public async Task<RegisterDoctorResult> Handle(RegisterDoctorCommand request, CancellationToken cancellationToken)
    {
        // 1. Генерируем уникальный Email
        var baseEmail = $"{Transliterate(request.FirstName).ToLower()}.{Transliterate(request.LastName).ToLower()}";
        var email = $"{baseEmail}@clinic.com";
        
        int counter = 1;
        while (await _accountRepo.AnyAsync(a => a.Email == email))
        {
            email = $"{baseEmail}{counter}@clinic.com";
            counter++;
        }

        var password = GenerateRandomPassword();

        // 2. Создаем аккаунт
        var account = new Account
        {
            Email = email,
            PasswordHash = _hasher.Hash(password),
            Role = RoleType.doctor, // Роль Врача
            Phone = request.Phone,
            EmailVerified = true,    
            PhoneVerified = true,    
            IdentityVerified = true  
        };

        await _accountRepo.AddAsync(account);
        await _accountRepo.SaveChangesAsync(); // Сохраняем, чтобы получить Account.Id

        // 3. Создаем профиль доктора и привязываем специализации
        var doctor = new Doctor
        {
            AccountId = account.Id,
            OfficeId = request.OfficeId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? "",
            Bio = request.Bio,
            HiredAt = DateOnly.FromDateTime(DateTime.UtcNow),
            AvatarUrl = "default_doctor.png", // Дефолтное фото
            RatingAvg = 0, // Стартовый рейтинг
            IsActive = true,
            
            // Заполняем связующую таблицу M2M через навигационное свойство
            DoctorSpecializations = request.Specializations.Select(s => new DoctorSpecialization
            {
                SpecializationId = s.SpecializationId,
                IsPrimary = s.IsPrimary,
                CareerStartDate = s.CareerStartDate
            }).ToList()
        };

        await _doctorRepo.AddAsync(doctor);
        await _doctorRepo.SaveChangesAsync();
        
        // 4. Отправляем письмо с паролем
        await _emailService.SendStaffCredentialsEmailAsync(email, password);
        
        return new RegisterDoctorResult(true, null, email, password);
    }

    // Вспомогательные методы генерации (такие же, как в RegisterRegistrar.cs)
    private static string GenerateRandomPassword()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%";
        var data = new byte[10];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);
        var result = new StringBuilder(10);
        foreach (var b in data) result.Append(chars[b % chars.Length]);
        return result.ToString();
    }

    private static string Transliterate(string text)
    {
        string[] rus = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
        string[] eng = { "a", "b", "v", "g", "d", "e", "e", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "h", "ts", "ch", "sh", "shch", "", "y", "", "e", "yu", "ya" };
        var result = text.ToLower();
        for (int i = 0; i < rus.Length; i++) result = result.Replace(rus[i], eng[i]);
        return result.Replace(" ", "");
    }
}
