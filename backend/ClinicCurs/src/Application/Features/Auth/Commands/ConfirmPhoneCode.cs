using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Auth.Commands;

public record ConfirmPhoneCodeResult(bool IsSuccess, string? ErrorMessage);

public record ConfirmPhoneCodeCommand(Guid AccountId, string Code) : IRequest<ConfirmPhoneCodeResult>;

public class ConfirmPhoneCodeHandler : IRequestHandler<ConfirmPhoneCodeCommand, ConfirmPhoneCodeResult>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IDistributedCache _cache;

    public ConfirmPhoneCodeHandler(IGenericRepository<Account> accountRepo, IDistributedCache cache)
    {
        _accountRepo = accountRepo;
        _cache = cache;
    }

    public async Task<ConfirmPhoneCodeResult> Handle(ConfirmPhoneCodeCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepo.GetByIdAsync(request.AccountId);
        if (account == null) 
            return new ConfirmPhoneCodeResult(false, "Аккаунт не найден.");

        if (account.PhoneVerified)
            return new ConfirmPhoneCodeResult(false, "Ваш номер телефона уже был подтвержден ранее.");

        var cacheKey = $"phone_verify_code_{request.AccountId}";
        var storedCode = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (storedCode == null)
            return new ConfirmPhoneCodeResult(false, "Срок действия кода истек или он не был запрошен.");

        if (storedCode != request.Code)
            return new ConfirmPhoneCodeResult(false, "Введен неверный код подтверждения.");

        var pendingPhone = await _cache.GetStringAsync($"pending_phone_{request.AccountId}", cancellationToken);

        if (!string.IsNullOrEmpty(pendingPhone))
        {
            // НОВАЯ ПРОВЕРКА (Защита от состояния гонки): 
            // Проверяем, не успел ли кто-то другой подтвердить этот номер, пока мы вводили код
            bool isPhoneTaken = await _accountRepo.AnyAsync(a => a.Phone == pendingPhone && a.PhoneVerified && a.Id != request.AccountId);
            if (isPhoneTaken)
            {
                return new ConfirmPhoneCodeResult(false, "К сожалению, этот номер уже был привязан к другому аккаунту.");
            }

            account.Phone = pendingPhone;
        }

        account.PhoneVerified = true;
        account.LastPhoneUpdate = DateTime.UtcNow;
        
        _accountRepo.Update(account);
        await _accountRepo.SaveChangesAsync();

        await _cache.RemoveAsync(cacheKey, cancellationToken);
        await _cache.RemoveAsync($"pending_phone_{request.AccountId}", cancellationToken);
        
        return new ConfirmPhoneCodeResult(true, null);
    }
}
