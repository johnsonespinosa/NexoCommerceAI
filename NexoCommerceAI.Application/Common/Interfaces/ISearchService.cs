namespace NexoCommerceAI.Application.Common.Interfaces;

public interface ISearchService<T> where T : class
{
    Task IndexAsync(T document, CancellationToken cancellationToken = default);
    Task IndexBulkAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> SearchAsync(string term, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
}