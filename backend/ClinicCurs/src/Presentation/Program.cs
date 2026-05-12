using Application;
using Infrastructure;
using Presentation.Endpoints;
using Presentation.Extension;
using Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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

app.Run();
