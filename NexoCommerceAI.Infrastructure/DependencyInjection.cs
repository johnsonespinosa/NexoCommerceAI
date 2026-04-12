using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Infrastructure.Data;
using NexoCommerceAI.Infrastructure.Data.Interceptors;
using NexoCommerceAI.Infrastructure.Data.Repositories;
using NexoCommerceAI.Infrastructure.Outbox;
using NexoCommerceAI.Infrastructure.Services;
using NexoCommerceAI.Infrastructure.Services.Background;
using NexoCommerceAI.Infrastructure.Services.EventBus;

namespace NexoCommerceAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment) 
    {
        // Database Context
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
                
            options.EnableSensitiveDataLogging(environment.IsDevelopment());
            options.EnableDetailedErrors(environment.IsDevelopment());
        });

        // Registrar IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped(typeof(IRepositoryAsync<>), typeof(RepositoryAsync<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>(); 
        services.AddScoped<ICartRepository, CartRepository>(); 
        services.AddScoped<IWishlistRepository, WishlistRepository>(); 

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
        
        services.AddScoped<ICartCacheService, CartCacheService>();
        
        // Elasticsearch
        services.Configure<ElasticSearchSettings>(
            configuration.GetSection("ElasticSearch"));
        services.AddSingleton(typeof(ISearchService<ProductDocument>), typeof(ElasticSearchService));

        // Outbox
        services.Configure<OutboxSettings>(
            configuration.GetSection("Outbox"));
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxBackgroundService>();
        
        // Event Bus
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        
        // Configuración de Cloudinary
        services.Configure<CloudinarySettings>(
            configuration.GetSection("Cloudinary"));
        
        // Registrar servicios de imágenes
        services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
        
        return services;
    }
}