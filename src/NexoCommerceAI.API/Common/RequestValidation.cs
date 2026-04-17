using System.ComponentModel.DataAnnotations;

namespace NexoCommerceAI.API.Common;

public static class RequestValidation
{
    public static IResult? Validate<T>(T request)
    {
        var validationContext = new ValidationContext(request!);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request!, validationContext, validationResults, true))
        {
            return Results.ValidationProblem(BuildValidationErrors(validationResults));
        }

        if (request is Contracts.Products.CreateProductRequest createRequest)
        {
            return ValidateCreateProductRequest(createRequest);
        }

        if (request is Contracts.Products.UpdateProductRequest updateRequest)
        {
            return ValidateUpdateProductRequest(updateRequest);
        }

        return null;
    }

    private static IResult ValidateCreateProductRequest(Contracts.Products.CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Name)] = ["Name is required."]
            });
        }

        if (request.CategoryId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.CategoryId)] = ["CategoryId is required."]
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Sku) && request.Sku.Trim().Length == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Sku)] = ["Sku cannot be empty."]
            });
        }

        return null;
    }

    private static IResult ValidateUpdateProductRequest(Contracts.Products.UpdateProductRequest request)
    {
        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Name)] = ["Name cannot be empty."]
            });
        }

        if (request.CategoryId.HasValue && request.CategoryId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.CategoryId)] = ["CategoryId cannot be empty."]
            });
        }

        if (request.Sku is not null && string.IsNullOrWhiteSpace(request.Sku))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Sku)] = ["Sku cannot be empty."]
            });
        }

        if (request.Price.HasValue && request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Price)] = ["Price must be greater than zero."]
            });
        }

        if (request.CompareAtPrice.HasValue && request.CompareAtPrice < 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.CompareAtPrice)] = ["CompareAtPrice must be zero or greater."]
            });
        }

        if (request.Stock.HasValue && request.Stock < 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Stock)] = ["Stock cannot be negative."]
            });
        }

        return Results.Ok();
    }

    private static IDictionary<string, string[]> BuildValidationErrors(IEnumerable<ValidationResult> validationResults)
    {
        return validationResults
            .GroupBy(result => result.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => grouping.Select(result => result.ErrorMessage ?? string.Empty).ToArray());
    }
}
