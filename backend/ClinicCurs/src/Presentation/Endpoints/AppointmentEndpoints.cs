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

        group.MapGet("/my", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();

            var appointments = await mediator.Send(new GetPatientAppointmentsQuery(accountId));
            return Results.Ok(appointments);
        }).RequireAuthorization("PatientOnly");

        group.MapPatch("/{id:guid}/cancel", async (Guid id, ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();

            var success = await mediator.Send(new CancelAppointmentCommand(accountId, id));
            return success ? Results.Ok(new { Message = "Запись отменена" }) : Results.BadRequest(new { error = "Невозможно отменить данную запись." });
        }).RequireAuthorization("PatientOnly");

        group.MapGet("/doctor/my", async ([FromQuery] string date, ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();

            if (!DateTime.TryParse(date, out var parsedDate)) parsedDate = DateTime.UtcNow;

            var appointments = await mediator.Send(new GetDoctorAppointmentsQuery(accountId, parsedDate));
            return Results.Ok(appointments);
        }).RequireAuthorization("DoctorOnly");

        // НОВЫЙ ЭНДПОИНТ: Завершение приема
        group.MapPost("/{id:guid}/complete", async (Guid id, [FromBody] CompleteAppointmentDto dto, ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();

            var result = await mediator.Send(new CompleteAppointmentCommand(accountId, id, dto.Complaints, dto.ObjectiveData, dto.Assessment, dto.Plan));
            
            return result.IsSuccess 
                ? Results.Ok(new { Message = "Прием завершен, медкарта обновлена." }) 
                : Results.BadRequest(new { error = result.ErrorMessage });
        }).RequireAuthorization("DoctorOnly");
        
        // НОВЫЙ ЭНДПОИНТ: Оставить отзыв
        group.MapPost("/{id:guid}/review", async (Guid id, [FromBody] CreateReviewDto dto, ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();

            var result = await mediator.Send(new CreateReviewCommand(accountId, id, dto.Rating, dto.Comment));
            
            return result.IsSuccess 
                ? Results.Ok(new { Message = "Отзыв успешно опубликован!" }) 
                : Results.BadRequest(new { error = result.ErrorMessage });
        }).RequireAuthorization("PatientOnly");
        
        // ЭНДПОИНТЫ РЕСЕПШЕНА:
        group.MapGet("/office/today", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(accountIdStr, out var accountId)) return Results.Unauthorized();
            
            var apps = await mediator.Send(new GetOfficeAppointmentsQuery(accountId));
            return Results.Ok(apps);
        }).RequireAuthorization("StaffOnly");

        group.MapPatch("/{id:guid}/confirm", async (Guid id, IMediator mediator) =>
        {
            var success = await mediator.Send(new ConfirmArrivalCommand(id));
            return success ? Results.Ok() : Results.BadRequest();
        }).RequireAuthorization("StaffOnly");
    }
}

public record BookAppointmentDto(Guid DoctorId, Guid TypeId, DateTime ScheduledStart);
public record CompleteAppointmentDto(string Complaints, string ObjectiveData, string Assessment, string Plan);
public record CreateReviewDto(int Rating, string Comment);
