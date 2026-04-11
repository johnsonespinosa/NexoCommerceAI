using MediatR;
using Microsoft.AspNetCore.Http;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Commands;

public record AddProductImageCommand(
    Guid ProductId,
    IFormFile Image,
    bool IsMain = false,
    int? DisplayOrder = null) : IRequest<ProductImageResponse>;