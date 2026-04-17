using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Infrastructure.Persistence;
using NexoCommerceAI.Infrastructure.Persistence.Interceptors;
using NexoCommerceAI.Infrastructure.Persistence.Repositories;
using NexoCommerceAI.Infrastructure.Services;

namespace NexoCommerceAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string postgresConnectionString, bool useInMemoryDatabase = false, string? inMemoryDatabaseName = null)
    {
        // Register DbContext save changes interceptor (in Interceptors namespace)
        services.AddScoped<ISaveChangesInterceptor, AuditSaveChangesInterceptor>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        // Register CurrentUserService and IHttpContextAccessor if needed
        services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register AuditLog repository
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // IUserProvider is implemented in API layer (HttpContextUserProvider) and will be resolved at runtime

        if (useInMemoryDatabase)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(inMemoryDatabaseName ?? "NexoCommerceAI"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(postgresConnectionString));
        }
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        return services;
    }
}
