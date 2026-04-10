using System.Diagnostics;
using System.Text;

namespace NexoCommerceAI.Api.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private const int MaxBodyLength = 4096; // Limitar a 4KB para evitar logs enormes

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        await LogRequest(context);
        
        // Log response
        await LogResponse(context, stopwatch);
    }
    
    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();
        
        var requestBody = await ReadRequestBody(context.Request);
        
        // Truncar body si es demasiado largo
        if (requestBody.Length > MaxBodyLength)
        {
            requestBody = requestBody[..MaxBodyLength] + "... (truncated)";
        }
        
        logger.LogInformation(
            "HTTP Request: {Method} {Path} - QueryString: {QueryString} - Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            requestBody);
        
        context.Request.Body.Position = 0;
    }
    
    private async Task LogResponse(HttpContext context, Stopwatch stopwatch)
    {
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        await next(context);
        
        stopwatch.Stop();
        
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
        
        // Truncar body si es demasiado largo
        if (responseBodyText.Length > MaxBodyLength)
        {
            responseBodyText = responseBodyText[..MaxBodyLength] + "... (truncated)";
        }
        
        logger.LogInformation(
            "HTTP Response: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms - Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            responseBodyText);
        
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }
    
    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        
        // Limitar longitud del body para logging
        if (body.Length > MaxBodyLength)
        {
            body = body[..MaxBodyLength] + "... (truncated)";
        }
        
        return body;
    }
}