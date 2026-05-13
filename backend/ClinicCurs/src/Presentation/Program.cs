using System.Text.Json.Serialization;
using Application;
using Infrastructure;
using Presentation.Endpoints;
using Presentation.Extension;
using Presentation.Middlewares;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCustomCors();

var app = builder.Build();

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

await app.Migrations();

app.MapAuthEndpoints();
app.MapVerificationEndpoints();
app.MapFilesEndpoints();
app.MapSystemEndpoints();
app.MapClinicEndpoints();
app.MapAdminEndpoints();
app.MapAppointmentEndpoints();
app.MapLabEndpoints();

app.Run();
