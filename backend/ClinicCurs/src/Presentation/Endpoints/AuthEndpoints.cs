using System.Security.Claims;
using Application.Features.Auth.Commands;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Endpoints;
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/register", async (RegisterAccountCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? Results.Ok(new { Message = "Регистрация успешна!", AccountId = result.AccountId }) 
                : Results.BadRequest(result.ErrorMessage);
        });

        group.MapPost("/login", async (LoginAccountCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            
            return result.IsSuccess 
                ? Results.Ok(new { AccessToken = result.AccessToken, RefreshToken = result.RefreshToken }) 
                : Results.Unauthorized();
        });

        // Обновление токена
        group.MapPost("/refresh", async (RefreshTokenCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            
            return result.IsSuccess 
                ? Results.Ok(new { AccessToken = result.AccessToken, RefreshToken = result.RefreshToken }) 
                : Results.BadRequest(new { error = result.ErrorMessage });
        });
        
        group.MapGet("/verify-email", async (string token, IMediator mediator) =>
        {
            if (string.IsNullOrWhiteSpace(token)) 
                return Results.BadRequest("Токен обязателен.");

            var command = new VerifyEmailCommand(token);
            var result = await mediator.Send(command);

            return result.IsSuccess 
                ? Results.Ok(new { Message = "Email успешно подтвержден!" }) 
                : Results.BadRequest(new { error = result.ErrorMessage });
        });
        
        group.MapPost("/phone/send-code", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdClaim, out var accountId)) return Results.Unauthorized();

            var result = await mediator.Send(new SendPhoneCodeCommand(accountId));
    
            return result.IsSuccess 
                ? Results.Ok(new { Message = "Код подтверждения отправлен." }) 
                : Results.BadRequest(new { error = result.ErrorMessage }); 
        }).RequireAuthorization();

        group.MapPost("/phone/change-request", async (string newPhone, ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdClaim, out var accountId)) return Results.Unauthorized();

            var result = await mediator.Send(new RequestPhoneChangeCommand(accountId, newPhone));
    
            return result.IsSuccess 
                ? Results.Ok(new { Message = "Код подтверждения отправлен на новый номер." }) 
                : Results.BadRequest(new { error = result.ErrorMessage });
        }).RequireAuthorization();
        
        // Подтвердить код
        group.MapPost("/phone/verify", async (string code, ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdClaim, out var accountId)) return Results.Unauthorized();

            var result = await mediator.Send(new ConfirmPhoneCodeCommand(accountId, code));
    
            return result.IsSuccess 
                ? Results.Ok(new { Message = "Номер телефона подтвержден." }) 
                : Results.BadRequest(new { error = result.ErrorMessage }); // Выведет "уже подтвержден" или "неверный код"
        }).RequireAuthorization();
        
        group.MapPost("/forgot-password", async (string email, IMediator mediator) =>
        {
            await mediator.Send(new ForgotPasswordCommand(email));
            return Results.Ok(new { Message = "Если такой Email существует, письмо со ссылкой отправлено." });
        });

        group.MapPost("/reset-password", async (ResetPasswordCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result 
                ? Results.Ok(new { Message = "Пароль успешно изменен." }) 
                : Results.BadRequest("Ссылка недействительна или истекла.");
        });
        
        group.MapPost("/logout", async (LogoutCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
    
            return result.IsSuccess 
                ? Results.Ok(new { Message = "Успешный выход из системы." }) 
                : Results.BadRequest(new { error = result.ErrorMessage });
        });
        
        group.MapGet("/me", async (
            ClaimsPrincipal user, 
            IGenericRepository<Account> accountRepo,
            IGenericRepository<Patient> patientRepo,
            IGenericRepository<Doctor> doctorRepo,
            IGenericRepository<Registrar> registrarRepo) =>
        {
            var accountIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdClaim, out var accountId)) return Results.Unauthorized();

            var account = await accountRepo.GetByIdAsync(accountId);
            if (account == null) return Results.NotFound();

            // Ищем URL аватара в зависимости от роли
            string? avatarUrl = null;
            if (account.Role == RoleType.patient) 
                avatarUrl = (await patientRepo.FirstOrDefaultAsync(p => p.AccountId == accountId))?.AvatarUrl;
            else if (account.Role == RoleType.doctor) 
                avatarUrl = (await doctorRepo.FirstOrDefaultAsync(d => d.AccountId == accountId))?.AvatarUrl;
            else if (account.Role == RoleType.registrar) 
                avatarUrl = (await registrarRepo.FirstOrDefaultAsync(r => r.AccountId == accountId))?.AvatarUrl;

            return Results.Ok(new 
            { 
                account.Email, 
                account.Phone, 
                account.EmailVerified, 
                account.PhoneVerified, 
                account.IdentityVerified,
                Role = account.Role.ToString(),
                AvatarUrl = avatarUrl // ТЕПЕРЬ ВОЗВРАЩАЕМ ФОТО
            });
        }).RequireAuthorization();
        
        // Самостоятельное удаление профиля пациентом
        group.MapDelete("/me", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId) || role == null) return Results.Unauthorized();

            var success = await mediator.Send(new Application.Features.Profiles.Commands.DeletePatientCommand(accountId, accountId, role));
            
            return success 
                ? Results.Ok(new { Message = "Ваш профиль успешно удален и анонимизирован." }) 
                : Results.BadRequest(new { error = "Ошибка при удалении профиля." });
        }).RequireAuthorization("PatientOnly");
        
        app.MapGet("/api/secure-data", (ClaimsPrincipal user) =>
            {
                var email = user.FindFirstValue(ClaimTypes.Email);
                var role = user.FindFirstValue(ClaimTypes.Role);
                return Results.Ok(new { Message = $"Привет, {email}! Роль: {role}." });
            })
            .RequireAuthorization()
            .WithTags("Test");
    }
}

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
