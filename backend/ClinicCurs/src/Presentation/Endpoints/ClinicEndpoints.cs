using Application.Features.Clinic.Queries;
using MediatR;

namespace Presentation.Endpoints;

public static class ClinicEndpoints
{
    public static void MapClinicEndpoints(this WebApplication app)
    {
        // Эндпоинты этой группы доступны всем пользователям (даже без логина)
        var group = app.MapGroup("/api/clinic").WithTags("Clinic Public Info");

        group.MapGet("/offices", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetActiveOfficesQuery());
            return Results.Ok(result);
        });
    }
}
