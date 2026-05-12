using ClinicCurs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Endpoints;

public static class SystemEndpoints
{
    public static void MapSystemEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", async (ClinicDbContext db) =>
        {
            try
            {
                var canConnect = await db.Database.CanConnectAsync();
                var count = await db.Accounts.CountAsync();
                return Results.Ok(new { status = "Healthy", accountsCount = count });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).WithTags("System");
    }
}
