// Application/Common/Exceptions/NotFoundException.cs
using FluentValidation.Results;

namespace NexoCommerceAI.Application.Common.Exceptions;

public abstract class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException : Exception
{
    // Constructor para cuando se busca por ID
    public NotFoundException(string name, object key) 
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
    
    // Constructor para mensaje personalizado
    public NotFoundException(string message) 
        : base(message)
    {
    }
    
    // Constructor para mensaje personalizado con inner exception
    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

public class BadRequestException(string message) : AppException(message, 400);

public class UnauthorizedException(string message) : AppException(message, 401);

public class ForbiddenException(string message) : AppException(message, 403);

public class ConflictException(string message) : AppException(message, 409);

public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}