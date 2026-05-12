using System.Security.Claims;
using Application.Features.Auth.Commands;
using Application.Interfaces;
using ClinicCurs.Infrastructure.Data;
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

        // Тестовый метод
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

public record RegisterRequest(string Email, string Password, string Phone);
public record LoginRequest(string Email, string Password);
