using MediatR;

namespace NexoCommerceAI.Domain.Events;

public sealed record StockRestockedEvent(
    Guid ProductId,
    string ProductName,
    int PreviousStock,
    int NewStock,
    int QuantityAdded
) : INotification;

public sealed record StockChangedEvent(
    Guid ProductId,
    string ProductName,
    int PreviousStock,
    int NewStock,
    int QuantityChanged
) : INotification;

public sealed record OutOfStockEvent(
    Guid ProductId,
    string ProductName
) : INotification;

public sealed record StockLowEvent(
    Guid ProductId,
    string ProductName,
    int CurrentStock,
    int Threshold
) : INotification;

public sealed record ProductImageAddedEvent(
    Guid ProductId,
    string ProductName,
    Guid ImageId,
    string ImageUrl,
    bool IsMain
) : INotification;

public sealed record ProductImageRemovedEvent(
    Guid ProductId,
    string ProductName,
    Guid ImageId,
    string ImageUrl,
    string? PublicId
) : INotification;

public sealed record ProductImageSetMainEvent(
    Guid ProductId,
    string ProductName,
    Guid ImageId,
    string ImageUrl,
    bool IsMain
) : INotification;
