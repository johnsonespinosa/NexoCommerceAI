using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Extensions;

public static class ProductQueryExtensions
{
    public static IQueryable<Product> ApplyFilters(this IQueryable<Product> query, PaginationParams pagination)
    {
        // Filtrar por término de búsqueda
        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var searchTerm = pagination.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                p.Sku.ToLower().Contains(searchTerm));
        }
        
        // Filtrar por categoría
        if (pagination.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == pagination.CategoryId.Value);
        }
        
        // Filtrar por destacado
        if (pagination.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == pagination.IsFeatured.Value);
        }
        
        // Filtrar por precio
        if (pagination.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= pagination.MinPrice.Value);
        }
        
        if (pagination.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= pagination.MaxPrice.Value);
        }
        
        // Filtrar por oferta
        if (pagination.IsOnSale.HasValue && pagination.IsOnSale.Value)
        {
            query = query.Where(p => p.CompareAtPrice != null && p.CompareAtPrice > p.Price);
        }
        
        // Solo productos activos y no eliminados
        query = query.Where(p => p.IsActive && !p.IsDeleted);
        
        return query;
    }
    
    public static IQueryable<Product> ApplySorting(this IQueryable<Product> query, PaginationParams pagination)
    {
        if (string.IsNullOrWhiteSpace(pagination.SortBy))
        {
            return query.OrderByDescending(p => p.CreatedAt);
        }
        
        var sortBy = pagination.SortBy.ToLower();
        var isDescending = pagination.SortDescending;
        
        return sortBy switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "stock" => isDescending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
            "createdat" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }
    
    public static IQueryable<Product> ApplyPagination(this IQueryable<Product> query, PaginationParams pagination)
    {
        return query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize);
    }
}