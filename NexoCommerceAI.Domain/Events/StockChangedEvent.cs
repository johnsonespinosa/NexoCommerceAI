using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

/// <summary>
/// Evento disparado cuando cambia el stock de un producto
/// </summary>
public sealed record StockChangedEvent(
    Guid ProductId,
    string ProductName,
    int PreviousStock,
    int NewStock,
    int QuantityChanged) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indica si el stock aumentó
    /// </summary>
    public bool IsIncrease => QuantityChanged > 0;
    
    /// <summary>
    /// Indica si el stock disminuyó
    /// </summary>
    public bool IsDecrease => QuantityChanged < 0;
    
    /// <summary>
    /// Valor absoluto del cambio
    /// </summary>
    public int AbsoluteChange => Math.Abs(QuantityChanged);
}