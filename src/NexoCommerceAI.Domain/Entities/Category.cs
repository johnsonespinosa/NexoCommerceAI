using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }

    private Category()
    {
        Name = string.Empty;
    }

    public Category(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();
    }
}
