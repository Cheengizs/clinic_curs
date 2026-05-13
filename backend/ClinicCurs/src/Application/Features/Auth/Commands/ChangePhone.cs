using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Auth.Commands;

public record ChangePhoneResult(bool IsSuccess, string? ErrorMessage);

public record RequestPhoneChangeCommand(Guid AccountId, string NewPhone) : IRequest<ChangePhoneResult>;

public class RequestPhoneChangeHandler : IRequestHandler<RequestPhoneChangeCommand, ChangePhoneResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IDistributedCache _cache;
    private readonly ISmsService _smsService;

    public RequestPhoneChangeHandler(IGenericRepository<Account> accountRepo, IDistributedCache cache, ISmsService smsService)
    {
        _accountRepo = accountRepo;
        _cache = cache;
        _smsService = smsService;
    }

    public async Task<ChangePhoneResult> Handle(RequestPhoneChangeCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepo.GetByIdAsync(request.AccountId);
        if (account == null) return new ChangePhoneResult(false, "Аккаунт не найден.");

        if (!account.EmailVerified) 
            return new ChangePhoneResult(false, "Сначала подтвердите Email.");

        // НОВАЯ ПРОВЕРКА: Занят ли номер другим аккаунтом?
        bool isPhoneTaken = await _accountRepo.AnyAsync(a => a.Phone == request.NewPhone && a.PhoneVerified);
        if (isPhoneTaken)
        {
            return new ChangePhoneResult(false, "Этот номер телефона уже привязан к другому аккаунту.");
        }

        if (account.LastPhoneUpdate.HasValue && (DateTime.UtcNow - account.LastPhoneUpdate.Value).TotalDays < 1)
        {
            var timeLeft = TimeSpan.FromDays(1) - (DateTime.UtcNow - account.LastPhoneUpdate.Value);
            return new ChangePhoneResult(false, $"Смена номера возможна раз в сутки. Попробуйте через {timeLeft.Hours}ч. {timeLeft.Minutes}мин.");
        }

        var code = new Random().Next(100000, 999999).ToString();
        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

        await _cache.SetStringAsync($"phone_verify_code_{request.AccountId}", code, cacheOptions, cancellationToken);
        await _cache.SetStringAsync($"pending_phone_{request.AccountId}", request.NewPhone, cacheOptions, cancellationToken);

        account.PhoneVerified = false; 
        _accountRepo.Update(account);
        await _accountRepo.SaveChangesAsync();
        
        await _smsService.SendVerificationCodeAsync(request.NewPhone, code);

        return new ChangePhoneResult(true, null);
    }
}
