using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToNoContent(this Result result)
    {
        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        return Results.NoContent();
    }

    public static IResult ToOk(this Result result)
    {
        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        return Results.Ok();
    }

    public static IResult ToOk<TValue>(this Result<TValue> result)
    {
        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        return Results.Ok(result.Value);
    }

    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create an error response from a successful result.");
        }

        return result.Error.Type switch
        {
            ErrorType.Validation => Results.BadRequest(new { error = result.Error.Description, code = result.Error.Code }),
            ErrorType.NotFound => Results.NotFound(new { error = result.Error.Description, code = result.Error.Code }),
            ErrorType.Conflict => Results.Conflict(new { error = result.Error.Description, code = result.Error.Code }),
            _ => Results.Problem(
                title: "Request failed",
                detail: result.Error.Description,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    public static IResult ToCreated<TValue>(this Result<TValue> result, Func<TValue, string> locationFactory)
    {
        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        return Results.Created(locationFactory(result.Value), result.Value);
    }
}
