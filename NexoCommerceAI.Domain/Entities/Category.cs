using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public ICollection<Product> Products { get; private set; } = new List<Product>();
    
    private Category() { }  // EF
    
    private Category(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }
    
    public static Category Create(string name, string? slug = null, Func<string, Task<bool>>? isSlugUnique = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required");
    
        if (!IsValidName(name))
            throw new ArgumentException("Category name contains invalid characters");
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required");

        var finalSlug = string.IsNullOrWhiteSpace(slug) 
            ? SlugGenerator.Generate(name) 
            : SlugGenerator.Generate(slug);

        if (string.IsNullOrWhiteSpace(finalSlug))
            throw new ArgumentException("Unable to generate valid slug from the provided name");
    
        // Validar unicidad del slug si se proporciona el delegado
        if (isSlugUnique != null && !isSlugUnique(finalSlug).Result)
            throw new ArgumentException($"Slug '{finalSlug}' already exists");

        return new Category(name, finalSlug);
    }
    
    private static bool IsValidName(string name)
    {
        // No permitir caracteres especiales que puedan causar problemas
        return !System.Text.RegularExpressions.Regex.IsMatch(name, @"[<>""'%&]");
    }
    
    // Overload para mantener compatibilidad
    public static Category Create(string name)
    {
        return Create(name, null);
    }
    
    public void Update(string name, string? slug = null, Func<string, Task<bool>>? isNameUnique = null, Func<string, Task<bool>>? isSlugUnique = null)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required");
    
        // Validar unicidad del nombre si se proporciona el delegado
        if (isNameUnique != null && !isNameUnique(name).Result)
            throw new ArgumentException($"Category name '{name}' already exists");
    
        Name = name;
    
        if (!string.IsNullOrWhiteSpace(slug))
        {
            if (!IsValidSlugFormat(slug))
                throw new ArgumentException("Slug contains invalid characters. Use only lowercase letters, numbers, and hyphens.");
        
            // Validar unicidad del slug si se proporciona el delegado
            if (isSlugUnique != null && !isSlugUnique(slug).Result)
                throw new ArgumentException($"Slug '{slug}' already exists");
        
            Slug = SlugGenerator.Generate(slug);
        }
        else
        {
            var newSlug = SlugGenerator.Generate(name);
        
            // Validar unicidad del slug generado
            if (isSlugUnique != null && !isSlugUnique(newSlug).Result)
                throw new ArgumentException($"Generated slug '{newSlug}' already exists");
        
            Slug = newSlug;
        }
    
        UpdatedAt = DateTime.UtcNow;
    }
    
    private static bool IsValidSlugFormat(string slug)
    {
        // Validar formato de slug: solo minúsculas, números y guiones
        return !string.IsNullOrWhiteSpace(slug) && 
               System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }
    
    public void UpdateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required");
        
        Slug = SlugGenerator.Generate(slug);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SoftDelete()
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        Products.Add(product);
    }
    
    public void RemoveProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (!Products.Contains(product))
            throw new InvalidOperationException($"Product '{product.Name}' is not in this category");
    
        Products.Remove(product);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public int GetProductCount()
    {
        return Products.Count(p => p is { IsDeleted: false, IsActive: true });
    }
}