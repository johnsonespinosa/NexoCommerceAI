namespace NexoCommerceAI.API.Middleware;

internal sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        const string headerName = "X-Correlation-Id";
        if (!context.Request.Headers.TryGetValue(headerName, out Microsoft.Extensions.Primitives.StringValues value) || string.IsNullOrWhiteSpace(value))
        {
            var newId = Guid.NewGuid().ToString();
            value = newId;
            context.Request.Headers[headerName] = value;
        }

        // Ensure response header is set
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(headerName))
            {
                context.Response.Headers[headerName] = context.Request.Headers[headerName].ToString();
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
