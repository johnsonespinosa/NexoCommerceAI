using AutoMapper;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.CategoryName, 
                opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategorySlug, 
                opt => opt.MapFrom(src => src.Category.Slug))
            .ForMember(dest => dest.DiscountPercentage, 
                opt => opt.MapFrom(src => src.GetDiscountPercentage()))
            .ForMember(dest => dest.IsOnSale, 
                opt => opt.MapFrom(src => src.IsOnSale()))
            .ForMember(dest => dest.IsInStock, 
                opt => opt.MapFrom(src => src.IsInStock()))
            .ForMember(dest => dest.CreatedAt, 
                opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, 
                opt => opt.MapFrom(src => src.UpdatedAt));
        
        // Mapeo para comandos
        CreateMap<CreateProductCommand, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());
        
        CreateMap<UpdateProductCommand, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
    }
}