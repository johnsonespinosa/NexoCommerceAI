using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public ICollection<User> Users { get; private set; } = new List<User>();
    
    private Role() { } // EF Core
    
    private Role(string name, string description)
    {
        Name = name;
        Description = description;
    }
    
    public static Role Create(string name, string description, Func<string, Task<bool>>? isNameUnique = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required");
    
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Role description is required");
    
        // Validar unicidad de nombre
        if (isNameUnique != null && !isNameUnique(name).Result)
            throw new ArgumentException($"Role name '{name}' already exists");
    
        return new Role(name, description);
    }
    
    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required");
            
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Role description is required");
            
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SoftDelete()
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool IsPredefinedRole()
    {
        return PredefinedRoles.All.Contains(Name);
    }
}

public static class PredefinedRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public const string Manager = "Manager";
    
    public static readonly IReadOnlyList<string> All = new[] { Admin, Customer, Manager };
}