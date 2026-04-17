using NexoCommerceAI.Application.Abstractions.Messaging;

namespace NexoCommerceAI.Application.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;
