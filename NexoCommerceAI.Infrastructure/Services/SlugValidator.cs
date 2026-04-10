using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data;

namespace NexoCommerceAI.Infrastructure.Services;

public class SlugValidator : ISlugValidator
{
    private readonly ApplicationDbContext _context;
    
    public SlugValidator(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> IsSlugUniqueAsync<T>(string slug, Guid? excludeId = null) where T : BaseEntity
    {
        if (typeof(T) == typeof(Category))
        {
            var query = _context.Set<Category>().AsQueryable();
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return !await query.AnyAsync(c => c.Slug == slug);
        }
        
        if (typeof(T) == typeof(Product))
        {
            var query = _context.Set<Product>().AsQueryable();
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return !await query.AnyAsync(p => p.Slug == slug);
        }
        
        return true;
    }
}