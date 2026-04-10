namespace NexoCommerceAI.Application.Common.Models;

public class PaginatedResult<T>(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
{
    public IReadOnlyList<T> Items { get; set; } = items;
    public int TotalCount { get; set; } = totalCount;
    public int PageNumber { get; set; } = pageNumber;
    public int PageSize { get; set; } = pageSize;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}