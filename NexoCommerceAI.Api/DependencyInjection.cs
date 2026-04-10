using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace NexoCommerceAI.Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services)
    {
        // API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
        
        // API Explorer (necesario para Swagger con versioning)
        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        
        // Controllers with ProblemDetails configuration
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ProblemDetails
                    {
                        Title = "Validation Error",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "See errors property for details",
                        Instance = context.HttpContext.Request.Path
                    };
                    return new BadRequestObjectResult(problemDetails);
                };
            });
        
        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", builder =>
            {
                builder.WithOrigins("https://localhost:4200") // frontend dev
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        // Swagger (si quieres agregar configuración avanzada)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Nexo Commerce AI API",
                Version = "v1",
                Description = "Nexo Commerce AI v1",
                Contact = new OpenApiContact
                {
                    Name = "Development Team",
                    Email = "dev@company.com"
                }
            });
            
            // Incluir comentarios XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            swaggerGenOptions.IncludeXmlComments(xmlPath);
    
            // Configurar seguridad JWT
            swaggerGenOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese el token JWT"
            });
        });
    }
}