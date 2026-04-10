using NexoCommerceAI.Api.Middleware;

namespace NexoCommerceAI.Api.Extensions;

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}