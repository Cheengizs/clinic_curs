using System.Security.Claims;
using Application.Features.Admin.Commands;
using Application.Features.Profiles.Commands;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class FilesEndpoints
{
    public static void MapFilesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/files").WithTags("Files & Profiles").RequireAuthorization();

        group.MapPost("/upload", async (IFormFile file, IBlobService blobService) =>
        {
            if (file == null || file.Length == 0) return Results.BadRequest("Файл пуст.");
            
            using var stream = file.OpenReadStream();
            var fileId = await blobService.UploadFileAsync(stream, file.FileName, file.ContentType, "images");
            
            // Отдаем клиенту только уникальное имя файла
            return Results.Ok(new { FileId = fileId });
        }).DisableAntiforgery();

        group.MapGet("/{fileId}", async (string fileId, IBlobService blobService) =>
        {
            try
            {
                var (stream, contentType, fileName) = await blobService.DownloadFileAsync(fileId, "images");
                return Results.File(stream, contentType); 
            }
            catch (FileNotFoundException)
            {
                return Results.NotFound();
            }
        }).AllowAnonymous();

        group.MapPatch("/avatar", async ([FromBody] UpdateAvatarDto dto, ClaimsPrincipal user, IMediator mediator) =>
        {
            var accountIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleStr = user.FindFirst(ClaimTypes.Role)?.Value;

            if (!Guid.TryParse(accountIdStr, out var accountId) || !Enum.TryParse<RoleType>(roleStr, out var role))
                return Results.Unauthorized();

            var result = await mediator.Send(new UpdateAvatarCommand(accountId, role, dto.FileId));
            return result ? Results.Ok("Аватар обновлен") : Results.BadRequest("Профиль не найден");
        });
        
        
    }
}

public record UpdateAvatarDto(string FileId);
