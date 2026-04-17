using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Attributes;

namespace NexoCommerceAI.Domain.Entities;

[IgnoreAudit]
public class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; }
    public string? PublicId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }

    private ProductImage()
    {
        ImageUrl = string.Empty;
    }

    public ProductImage(Guid productId, string imageUrl, string? publicId = null, int displayOrder = 0, bool isMain = false)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId is required", nameof(productId));
        if (string.IsNullOrWhiteSpace(imageUrl)) throw new ArgumentException("ImageUrl is required", nameof(imageUrl));
        if (displayOrder < 0) throw new ArgumentException("DisplayOrder cannot be negative", nameof(displayOrder));

        ProductId = productId;
        ImageUrl = imageUrl.Trim();
        PublicId = publicId?.Trim();
        DisplayOrder = displayOrder;
        IsMain = isMain;
    }

    public void SetAsMain() => IsMain = true;
    public void UnsetAsMain() => IsMain = false;
}
