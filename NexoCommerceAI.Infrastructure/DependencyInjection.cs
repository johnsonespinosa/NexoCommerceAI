using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;
using NexoCommerceAI.Infrastructure.Data;
using NexoCommerceAI.Infrastructure.Data.Interceptors;
using NexoCommerceAI.Infrastructure.Data.Repositories;
using NexoCommerceAI.Infrastructure.Services;

namespace NexoCommerceAI.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment) 
    {
        // Database Context
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
    
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure();
            });
            options.AddInterceptors(interceptor);
                
            // Usar el environment en lugar de builder
            options.EnableSensitiveDataLogging(environment.IsDevelopment());
            options.EnableDetailedErrors(environment.IsDevelopment());
        });

        // Repositories
        services.AddScoped(typeof(IRepositoryAsync<>), typeof(RepositoryAsync<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>(); 

        // Infrastructure Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        
        // HTTP Context Accessor
        services.AddHttpContextAccessor();
            
        // Configuración de caché
        services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            
        // Registrar servicio de caché
        if (configuration.GetValue<bool>("CacheSettings:UseRedis"))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetValue<string>("CacheSettings:RedisConnectionString");
                options.InstanceName = "NexoCommerceAI_";
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

    }
}