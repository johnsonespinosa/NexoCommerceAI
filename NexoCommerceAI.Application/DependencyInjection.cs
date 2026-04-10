using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Application.Common.Behaviors;
using NexoCommerceAI.Application.Features.Products.Mappings;

namespace NexoCommerceAI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration => {
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Configurar AutoMapper
        services.AddAutoMapper(typeof(ProductProfile));

        return services;
    }
}