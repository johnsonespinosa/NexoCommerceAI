namespace NexoCommerceAI.Application.Common.Models;

public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    
    public int PageNumber { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public Guid? CategoryId { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsOnSale { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}