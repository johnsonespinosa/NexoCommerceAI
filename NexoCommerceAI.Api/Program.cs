using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
    options.RejectionStatusCode = 429;
    
    // Política global
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                     httpContext.Request.Headers.Host.ToString();
        
        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
    
    // Política específica para login
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    
    // Política específica para register
    options.AddFixedWindowLimiter("register", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueLimit = 0;
    });
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers.RetryAfter = "60";
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString(CultureInfo.InvariantCulture);
        }
        
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
    };
});

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// ===========================
// Middleware Pipeline
// ===========================
app.UseRateLimiter();

// Configurar Swagger solo en desarrollo
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(swaggerUiOptions =>
    {
        swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "NexoCommerceAI API v1");
        swaggerUiOptions.RoutePrefix = "swagger";
        swaggerUiOptions.DocumentTitle = "NexoCommerceAI API Documentation";
        swaggerUiOptions.DisplayRequestDuration();
        swaggerUiOptions.EnableTryItOutByDefault();
        swaggerUiOptions.EnableDeepLinking();
        swaggerUiOptions.ShowExtensions();
    });
}

// Global Exception Handling
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRequestLogging();

// Health Checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration
            }),
            totalDuration = report.TotalDuration
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

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