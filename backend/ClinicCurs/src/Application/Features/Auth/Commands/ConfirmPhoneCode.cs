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
        // 1. Ищем аккаунт в базе данных
        var account = await _accountRepo.GetByIdAsync(request.AccountId);
        if (account == null) 
            return new ConfirmPhoneCodeResult(false, "Аккаунт не найден.");

        // 2. НОВАЯ ПРОВЕРКА: Если телефон уже подтвержден — выходим с ошибкой
        if (account.PhoneVerified)
        {
            return new ConfirmPhoneCodeResult(false, "Ваш номер телефона уже был подтвержден ранее.");
        }

        // 3. Достаем код из Redis
        var cacheKey = $"phone_verify_code_{request.AccountId}";
        var storedCode = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (storedCode == null)
        {
            return new ConfirmPhoneCodeResult(false, "Срок действия кода истек или он не был запрошен.");
        }

        if (storedCode != request.Code)
        {
            return new ConfirmPhoneCodeResult(false, "Введен неверный код подтверждения.");
        }

        // 4. Пытаемся достать НОВЫЙ номер из Redis (если была смена номера)
        var pendingPhone = await _cache.GetStringAsync($"pending_phone_{request.AccountId}", cancellationToken);

        if (!string.IsNullOrEmpty(pendingPhone))
        {
            account.Phone = pendingPhone;
        }

        // 5. Обновляем статус аккаунта
        account.PhoneVerified = true;
        account.LastPhoneUpdate = DateTime.UtcNow;
        
        _accountRepo.Update(account);
        await _accountRepo.SaveChangesAsync();

        // 6. Очищаем временные данные в Redis
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        await _cache.RemoveAsync($"pending_phone_{request.AccountId}", cancellationToken);
        
        return new ConfirmPhoneCodeResult(true, null);
    }
}
