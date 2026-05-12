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
                ? Results.Ok(new { Token = result.Token }) 
                : Results.Unauthorized();
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

public record RegisterRequest(string Email, string Password, string Phone);
public record LoginRequest(string Email, string Password);
