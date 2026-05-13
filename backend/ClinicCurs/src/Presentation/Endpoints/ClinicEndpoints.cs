using System.Security.Claims;
using Application.Features.Clinic.Queries;
using Application.Interfaces.Repositories;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class ClinicEndpoints
{
    public static void MapClinicEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/clinic").WithTags("Clinic Public Info");

        group.MapGet("/offices", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetActiveOfficesQuery());
            return Results.Ok(result);
        });
        
        group.MapGet("/specializations", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllSpecializationsQuery());
            return Results.Ok(result);
        });
        
        group.MapGet("/doctors", async (
            [FromQuery] Guid? officeId, 
            [FromQuery] Guid? specializationId, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            IMediator mediator = null!) =>
        {
            var result = await mediator.Send(new GetDoctorsQuery(officeId, specializationId, pageNumber, pageSize));
            return Results.Ok(result);
        });
        
        group.MapGet("/my-history", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();

            var history = await mediator.Send(new GetMedicalHistoryQuery(accountId));
            return Results.Ok(history);
        }).RequireAuthorization("PatientOnly");
        
        group.MapGet("/appointment-types", async (IGenericRepository<AppointmentType> repo) =>
        {
            var types = await repo.GetAllAsync();
            return Results.Ok(types);
        });
    }
}
