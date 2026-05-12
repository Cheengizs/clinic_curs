using System.Security.Claims;
using Application.Features.Auth.Commands;
using Application.Interfaces;
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
