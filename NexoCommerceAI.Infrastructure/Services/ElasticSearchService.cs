using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;
using NexoCommerceAI.Application.Features.Products.Models;
using System.Text.Json;

namespace NexoCommerceAI.Infrastructure.Services;

public class ElasticSearchService : ISearchService<ProductDocument>
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchService> _logger;
    private readonly string _indexName;
    
    public ElasticSearchService(
        IOptions<ElasticSearchSettings> settings,
        ILogger<ElasticSearchService> logger)
    {
        var settings1 = settings.Value;
        _logger = logger;
        _indexName = settings1.IndexName;
        
        var clientSettings = new ElasticsearchClientSettings(new Uri(settings1.Url))
            .DefaultIndex(_indexName);
        
        if (settings1.EnableDebugMode)
        {
            clientSettings.EnableDebugMode();
        }
        
        _client = new ElasticsearchClient(clientSettings);
        
        EnsureIndexExists().Wait();
    }
    
    private async Task EnsureIndexExists()
    {
        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(_indexName);
            
            if (!existsResponse.Exists)
            {
                var createResponse = await _client.Indices.CreateAsync(_indexName);
                
                if (!createResponse.IsValidResponse)
                {
                    _logger.LogError("Failed to create Elasticsearch index: {Error}", createResponse.DebugInformation);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring Elasticsearch index exists");
        }
    }
    
    public async Task IndexAsync(ProductDocument document, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.IndexAsync(document, idx => idx
                .Index(_indexName)
                .Id(document.Id.ToString()), cancellationToken);
            
            if (!response.IsValidResponse)
            {
                _logger.LogError("Failed to index document {Id}: {Error}", document.Id, response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document {Id}", document.Id);
        }
    }
    
    public async Task IndexBulkAsync(IEnumerable<ProductDocument> documents, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.BulkAsync(b => b
                .Index(_indexName)
                .IndexMany(documents), cancellationToken);
            
            if (!response.IsValidResponse)
            {
                _logger.LogError("Failed to bulk index documents: {Error}", response.DebugInformation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk indexing documents");
        }
    }
    
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteAsync<ProductDocument>(id, d => d.Index(_indexName), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id}", id);
        }
    }
    
    public async Task<IReadOnlyList<ProductDocument>> SearchAsync(string term, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new
            {
                from = skip,
                size = take,
                query = new
                {
                    multi_match = new
                    {
                        query = term,
                        fields = new[] { "name^3", "description^2", "sku^4" },
                        fuzziness = "AUTO"
                    }
                }
            };
            
            var jsonQuery = JsonSerializer.Serialize(query);
            
            var response = await _client.SearchAsync<ProductDocument>(jsonQuery, cancellationToken);
            
            if (!response.IsValidResponse)
            {
                _logger.LogError("Search failed: {Error}", response.DebugInformation);
                return new List<ProductDocument>();
            }
            
            return response.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching with term: {Term}", term);
            return new List<ProductDocument>();
        }
    }
}