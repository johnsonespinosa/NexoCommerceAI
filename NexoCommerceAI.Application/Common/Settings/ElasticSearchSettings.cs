namespace NexoCommerceAI.Application.Common.Settings;

public class ElasticSearchSettings
{
    /// <summary>
    /// URL del servidor Elasticsearch (ej: http://localhost:9200)
    /// </summary>
    public string Url { get; init; } = "http://localhost:9200";
    
    /// <summary>
    /// Nombre del índice principal para productos
    /// </summary>
    public string IndexName { get; init; } = "nexocommerce_products";
    
    /// <summary>
    /// Nombre de usuario para autenticación (si es requerida)
    /// </summary>
    public string? UserName { get; init; }
    
    /// <summary>
    /// Contraseña para autenticación (si es requerida)
    /// </summary>
    public string? Password { get; init; }
    
    /// <summary>
    /// Número de shards primarios
    /// </summary>
    public int NumberOfShards { get; init; } = 1;
    
    /// <summary>
    /// Número de réplicas
    /// </summary>
    public int NumberOfReplicas { get; init; } = 1;
    
    /// <summary>
    /// Timeout de conexión en segundos
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;
    
    /// <summary>
    /// Habilitar debug mode para desarrollo
    /// </summary>
    public bool EnableDebugMode { get; init; }
}