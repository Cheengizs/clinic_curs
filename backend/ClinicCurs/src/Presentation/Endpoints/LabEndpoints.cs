using System.Security.Claims;
using Application.Features.Labs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class LabEndpoints
{
    public static void MapLabEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/labs").WithTags("Labs");

        group.MapGet("/tests", async (IMediator mediator) => Results.Ok(await mediator.Send(new GetLabTestsQuery())));

        group.MapGet("/my", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();
            return Results.Ok(await mediator.Send(new GetMyLabResultsQuery(accountId)));
        }).RequireAuthorization("PatientOnly");

        group.MapPost("/patient/{patientId:guid}", async (Guid patientId, [FromBody] AddLabResultDto dto, ClaimsPrincipal user, IMediator mediator) =>
        {
            var staffIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(staffIdStr, out var staffId)) return Results.Unauthorized();
            
            var success = await mediator.Send(new AddLabResultCommand(staffId, patientId, dto.TestId, dto.FileId));
            return success ? Results.Ok(new { Message = "Результат анализа прикреплен к карте." }) : Results.BadRequest();
        }).RequireAuthorization("StaffOnly"); // Доступно Врачам, Админам и Регистраторам
    }
}

public record AddLabResultDto(Guid TestId, string FileId);
