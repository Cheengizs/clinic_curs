using Application.Features.Admin.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin Management")
            .RequireAuthorization("AdminOnly");

        group.MapPost("/offices", async ([FromBody] CreateOfficeCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(new { OfficeId = id });
        });

        group.MapPatch("/offices/{officeId:guid}/photo", async (Guid officeId, [FromBody] UpdateOfficePhotoDto dto, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateOfficePhotoCommand(officeId, dto.FileId));
            
            return result 
                ? Results.Ok(new { Message = "Фото офиса успешно обновлено." }) 
                : Results.NotFound(new { error = "Офис не найден." });
        });

        group.MapPost("/registrars", async ([FromBody] RegisterRegistrarCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            
            return result.IsSuccess 
                ? Results.Ok(new 
                { 
                    Message = "Регистратор успешно добавлен в систему.", 
                    Email = result.Email, 
                    Password = result.Password 
                }) 
                : Results.BadRequest(new { error = result.ErrorMessage });
        });
    }
}

public record UpdateOfficePhotoDto(string FileId);
