using System.Security.Claims;
using Application.Features.Appointments.Commands;
using Application.Features.Appointments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        // ДОБАВЛЕН typeId в Query
        group.MapGet("/slots", async ([FromQuery] Guid doctorId, [FromQuery] string date, [FromQuery] Guid typeId, IMediator mediator) =>
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return Results.BadRequest(new { error = "Неверный формат даты" });

            var slots = await mediator.Send(new GetAvailableSlotsQuery(doctorId, parsedDate, typeId));
            return Results.Ok(slots);
        });

        group.MapPost("/book", async ([FromBody] BookAppointmentDto dto, ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();

            var result = await mediator.Send(new BookAppointmentCommand(accountId, dto.DoctorId, dto.TypeId, dto.ScheduledStart));

            return result.IsSuccess 
                ? Results.Ok(new { Message = "Вы успешно записаны на прием!" }) 
                : Results.BadRequest(new { error = result.ErrorMessage });
        }).RequireAuthorization("PatientOnly");
    }
}

public record BookAppointmentDto(Guid DoctorId, Guid TypeId, DateTime ScheduledStart);
