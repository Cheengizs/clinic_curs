using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Auth.Commands;

public record SendPhoneCodeResult(bool IsSuccess, string? ErrorMessage);

public record SendPhoneCodeCommand(Guid AccountId) : IRequest<SendPhoneCodeResult>;

public class SendPhoneCodeHandler : IRequestHandler<SendPhoneCodeCommand, SendPhoneCodeResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IDistributedCache _cache;
    private readonly ISmsService _smsService;

    public SendPhoneCodeHandler(IGenericRepository<Account> accountRepo, IDistributedCache cache, ISmsService smsService)
    {
        _accountRepo = accountRepo;
        _cache = cache;
        _smsService = smsService;
    }

    public async Task<SendPhoneCodeResult> Handle(SendPhoneCodeCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepo.GetByIdAsync(request.AccountId);
    
        if (account == null) 
            return new SendPhoneCodeResult(false, "Аккаунт не найден.");

        if (!account.EmailVerified)
        {
            return new SendPhoneCodeResult(false, "Необходимо подтвердить Email перед верификацией номера телефона.");
        }

        if (string.IsNullOrEmpty(account.Phone)) 
            return new SendPhoneCodeResult(false, "В профиле не указан номер телефона.");

        if (account.PhoneVerified)
        {
            return new SendPhoneCodeResult(false, "Ваш номер телефона уже подтвержден.");
        }

        var code = new Random().Next(100000, 999999).ToString();
        
        var cacheKey = $"phone_verify_code_{request.AccountId}";
        await _cache.SetStringAsync(cacheKey, code, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        }, cancellationToken);

        await _smsService.SendVerificationCodeAsync(account.Phone, code);

        return new SendPhoneCodeResult(true, null);
    }
}
