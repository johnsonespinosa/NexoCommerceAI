using NexoCommerceAI.Application.Abstractions.Messaging;

namespace NexoCommerceAI.Application.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
