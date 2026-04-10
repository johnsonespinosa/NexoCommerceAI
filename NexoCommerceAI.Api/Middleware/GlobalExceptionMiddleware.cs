using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Common.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace NexoCommerceAI.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Validation Error";
                problemDetails.Detail = "One or more validation errors occurred";
                problemDetails.Extensions["errors"] = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                logger.LogWarning(exception, "Validation error: {Errors}", validationEx.Errors);
                break;

            case BadRequestException badRequestEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = badRequestEx.Message;
                logger.LogWarning(exception, "Bad request: {Message}", badRequestEx.Message);
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = unauthorizedEx.Message;
                logger.LogWarning(exception, "Unauthorized: {Message}", unauthorizedEx.Message);
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = StatusCodes.Status403Forbidden;
                problemDetails.Title = "Forbidden";
                problemDetails.Detail = forbiddenEx.Message;
                logger.LogWarning(exception, "Forbidden: {Message}", forbiddenEx.Message);
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Title = "Not Found";
                problemDetails.Detail = notFoundEx.Message;
                logger.LogWarning(exception, "Not found: {Message}", notFoundEx.Message);
                break;

            case ConflictException conflictEx:
                response.StatusCode = StatusCodes.Status409Conflict;
                problemDetails.Title = "Conflict";
                problemDetails.Detail = conflictEx.Message;
                logger.LogWarning(exception, "Conflict: {Message}", conflictEx.Message);
                break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An error occurred while processing your request";
                logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        problemDetails.Status = response.StatusCode;
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var jsonResponse = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await response.WriteAsync(jsonResponse);
    }
}