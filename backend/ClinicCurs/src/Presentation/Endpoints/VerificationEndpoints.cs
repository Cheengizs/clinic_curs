using System.Security.Claims;
using Application.Features.Verification.Commands;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class VerificationEndpoints
{
    public static void MapVerificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/verification").WithTags("Verification");

        group.MapPost("/submit", async ([FromBody] SubmitVerificationDto dto, ClaimsPrincipal user, IMediator mediator) =>
            {
                var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();

                var command = new SubmitVerificationRequestCommand(
                    accountId,
                    dto.FirstName,
                    dto.LastName,
                    dto.MiddleName,
                    dto.BirthDate,
                    dto.PassportSeriesNumber,
                    dto.PersonalNumber,
                    dto.OfficeId,       
                    dto.ScheduledAt     
                );

                var result = await mediator.Send(command);

                return result.IsSuccess 
                    ? Results.Ok(new { Message = "Вы успешно записаны на верификацию в клинику." }) 
                    : Results.BadRequest(new { error = result.ErrorMessage });
            })
            .RequireAuthorization("PatientOnly");
        
        group.MapGet("/pending", async (IGenericRepository<VerificationRequest> requestRepo) =>
            {
                var pendingRequests = await requestRepo.FindAsync(r => r.Status == VerificationStatuses.wait);
            
                var response = pendingRequests.Select(r => new
                {
                    r.Id,
                    r.FirstName,
                    r.LastName,
                    r.MiddleName,
                    r.BirthDate,
                    r.PassportSeriesNumber,
                    r.PersonalNumber,
                    r.CreatedAt
                });

                return Results.Ok(response);
            })
            .RequireAuthorization("StaffOnly"); 
        
        group.MapPost("/{requestId:guid}/process", async (Guid requestId, [FromBody] ProcessVerificationDto dto, ClaimsPrincipal user, IMediator mediator) =>
            {
                var registrarAccountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(registrarAccountIdStr, out var registrarAccountId)) return Results.Unauthorized();

                var command = new ProcessVerificationRequestCommand(requestId, registrarAccountId, dto.IsApproved);
            
                var result = await mediator.Send(command);

                return result.IsSuccess 
                    ? Results.Ok(new { Message = "Заявка успешно обработана." }) 
                    : Results.BadRequest(new { error = result.ErrorMessage });
            })
            .RequireAuthorization("StaffOnly"); // Политика из AuthExtensions
    }
}

public record ProcessVerificationDto(bool IsApproved);

public record SubmitVerificationDto(
    string FirstName, 
    string LastName, 
    string MiddleName, 
    DateOnly BirthDate, 
    string PassportSeriesNumber, 
    string PersonalNumber,
    Guid OfficeId,          
    DateTime ScheduledAt    
);
