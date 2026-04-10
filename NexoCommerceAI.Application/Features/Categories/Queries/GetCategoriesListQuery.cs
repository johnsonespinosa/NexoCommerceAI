using MediatR;
using NexoCommerceAI.Application.Features.Categories.Models;

namespace NexoCommerceAI.Application.Features.Categories.Queries;

public record GetAllCategoriesQuery : IRequest<List<CategoryResponse>>;