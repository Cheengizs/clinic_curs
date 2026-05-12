using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;

namespace Application.Features.Admin.Commands;
public record RegisterRegistrarResult(bool IsSuccess, string? ErrorMessage, string? Email, string? Password);

public record RegisterRegistrarCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Phone,
    Guid OfficeId) : IRequest<RegisterRegistrarResult>;

public class RegisterRegistrarHandler : IRequestHandler<RegisterRegistrarCommand, RegisterRegistrarResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IGenericRepository<Registrar> _registrarRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IEmailService _emailService; 

    public RegisterRegistrarHandler(
        IGenericRepository<Account> accountRepo, 
        IGenericRepository<Registrar> registrarRepo, 
        IPasswordHasher hasher,
        IEmailService emailService) // Добавь в конструктор
    {
        _accountRepo = accountRepo;
        _registrarRepo = registrarRepo;
        _hasher = hasher;
        _emailService = emailService;
    }

    public async Task<RegisterRegistrarResult> Handle(RegisterRegistrarCommand request, CancellationToken cancellationToken)
    {
        var baseEmail = $"{Transliterate(request.FirstName).ToLower()}.{Transliterate(request.LastName).ToLower()}";
        var email = $"{baseEmail}@clinic.com";
        
        int counter = 1;
        while (await _accountRepo.AnyAsync(a => a.Email == email))
        {
            email = $"{baseEmail}{counter}@clinic.com";
            counter++;
        }

        var password = GenerateRandomPassword();

        var account = new Account
        {
            Email = email,
            PasswordHash = _hasher.Hash(password),
            Role = RoleType.registrar,
            Phone = request.Phone,
            EmailVerified = true,    
            PhoneVerified = true,    
            IdentityVerified = true  
        };

        await _accountRepo.AddAsync(account);
        await _accountRepo.SaveChangesAsync(); 

        var registrar = new Registrar
        {
            AccountId = account.Id,
            OfficeId = request.OfficeId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName ?? "",
            AvatarUrl = "default_staff.png",
            IsActive = true
        };

        await _registrarRepo.AddAsync(registrar);
        await _registrarRepo.SaveChangesAsync();
        await _emailService.SendStaffCredentialsEmailAsync(email, password);
        
        return new RegisterRegistrarResult(true, null, email, password);
    }

    private static string GenerateRandomPassword()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%";
        var data = new byte[10];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);
        var result = new StringBuilder(10);
        foreach (var b in data)
        {
            result.Append(chars[b % chars.Length]);
        }
        return result.ToString();
    }

    private static string Transliterate(string text)
    {
        string[] rus = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
        string[] eng = { "a", "b", "v", "g", "d", "e", "e", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "h", "ts", "ch", "sh", "shch", "", "y", "", "e", "yu", "ya" };
        
        var result = text.ToLower();
        for (int i = 0; i < rus.Length; i++)
        {
            result = result.Replace(rus[i], eng[i]);
        }
        return result.Replace(" ", "");
    }
}
