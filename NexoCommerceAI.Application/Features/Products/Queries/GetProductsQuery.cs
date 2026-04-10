using MediatR;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Products.DTOs;

namespace NexoCommerceAI.Application.Features.Products.Queries;

public record GetProductByIdQuery() : IRequest<PagedResponse<ProductListItemDto>>;