using FluentValidation;
using MediatR;
using NexoCommerceAI.Application.Common;
using System.Reflection;

namespace NexoCommerceAI.Application.Pipeline;

internal sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToArray();

        if (failures.Any())
        {
            var errors = failures.Select(f => new Error(f.ErrorCode ?? "Validation", f.ErrorMessage, ErrorType.Validation)).ToArray();
            var validationError = new ValidationError(errors);

            // We need to convert to either Result or Result<TValue>
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(validationError);
            }

            var genericResultType = typeof(TResponse);

            // TResponse is Result<TValue>
            var method = typeof(Result).GetMethod("Failure", BindingFlags.Public | BindingFlags.Static)?.MakeGenericMethod(genericResultType.GetGenericArguments()[0]);
            var result = method?.Invoke(null, new object[] { validationError });

            return (TResponse)result!;
        }

        return await next();
    }
}
