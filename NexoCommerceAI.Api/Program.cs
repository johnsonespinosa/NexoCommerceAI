using Microsoft.AspNetCore.RateLimiting;
using NexoCommerceAI.Api;
using NexoCommerceAI.Api.Extensions;
using NexoCommerceAI.Api.Middleware;
using NexoCommerceAI.Application;
using NexoCommerceAI.Infrastructure;
using NexoCommerceAI.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// Serilog Configuration
// ===========================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuración de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ===========================
// Configure Services
// ===========================
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApiServices();
builder.Services.AddApiSecurity(builder.Configuration);

// ===========================
// Rate Limiting (nativo .NET 8)
// ===========================
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// ===========================
// Middleware Pipeline
// ===========================
app.UseRateLimiter();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(swaggerUiOptions =>
    {
        // Endpoint principal
        swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API v1");
            
        // Configuración UI
        swaggerUiOptions.RoutePrefix = string.Empty;              // Raíz del sitio
        swaggerUiOptions.DocumentTitle = "Ecommerce API Documentation";
        swaggerUiOptions.DefaultModelsExpandDepth(-1);            // Esconde modelos
        swaggerUiOptions.DisplayRequestDuration();                // Muestra tiempo de respuesta
        swaggerUiOptions.EnableTryItOutByDefault();               // Try it out activado
        swaggerUiOptions.EnableDeepLinking();                     // Deep linking para endpoints
        swaggerUiOptions.ShowExtensions();                        // Muestra extensiones
    });
}

// Global Exception Handling
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRequestLogging();

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===========================
// Database Initialization
// ===========================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DatabaseInitializer.InitializeAsync(services);
}

// ===========================
// Run
// ===========================
await app.RunAsync();

// Make Program accessible to tests
public partial class Program { }