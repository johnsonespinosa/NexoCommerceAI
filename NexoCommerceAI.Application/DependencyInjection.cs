using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Application.Common.Behaviors;
using NexoCommerceAI.Application.Features.Products.Events.Handlers;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration => {
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatcherBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient<INotificationHandler<StockLowEvent>, StockLowEventHandler>();
        services.AddTransient<INotificationHandler<OutOfStockEvent>, OutOfStockEventHandler>();
        services.AddTransient<INotificationHandler<StockRestockedEvent>, StockRestockedEventHandler>();
        services.AddTransient<INotificationHandler<ProductImageAddedEvent>, ProductImageAddedEventHandler>();
        services.AddTransient<INotificationHandler<ProductImageRemovedEvent>, ProductImageRemovedEventHandler>();
        services.AddTransient<INotificationHandler<ProductImageSetMainEvent>, ProductImageSetMainEventHandler>();

        return services;
    }
}