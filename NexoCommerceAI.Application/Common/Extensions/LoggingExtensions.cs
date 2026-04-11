using Microsoft.Extensions.Logging;

namespace NexoCommerceAI.Application.Common.Extensions;

public static class LoggingExtensions
{
    // Create Product
    private static readonly Action<ILogger, string, Guid, decimal, int, Exception?> ProductCreationStartedAction =
        LoggerMessage.Define<string, Guid, decimal, int>(
            LogLevel.Information,
            new EventId(1, "CreateProductStarted"),
            "Creating product: {ProductName}, Category: {CategoryId}, Price: {Price}, Stock: {Stock}");

    private static readonly Action<ILogger, Guid, string, Exception?> ProductCreatedAction =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(2, "ProductCreated"),
            "Product created successfully: {ProductId} - {ProductName}");

    // Update Product
    private static readonly Action<ILogger, Guid, Exception?> ProductUpdateStartedAction =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(3, "UpdateProductStarted"),
            "Updating product: {ProductId}");

    private static readonly Action<ILogger, Guid, string, Exception?> ProductUpdatedAction =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(4, "ProductUpdated"),
            "Product updated successfully: {ProductId} - {ProductName}");

    // Delete Product
    private static readonly Action<ILogger, Guid, Exception?> ProductDeleteStartedAction =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(5, "DeleteProductStarted"),
            "Deleting product: {ProductId}");

    private static readonly Action<ILogger, Guid, string, Exception?> ProductDeletedAction =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(6, "ProductDeleted"),
            "Product deleted successfully: {ProductId} - {ProductName}");

    private static readonly Action<ILogger, Guid, int, Exception?> ProductDeleteBlockedAction =
        LoggerMessage.Define<Guid, int>(
            LogLevel.Warning,
            new EventId(7, "ProductDeleteBlocked"),
            "Cannot delete product with positive stock: {ProductId} - Stock: {Stock}");

    // Get Product By Id
    private static readonly Action<ILogger, Guid, Exception?> GetProductByIdStartedAction =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(8, "GetProductByIdStarted"),
            "Getting product by ID: {ProductId}");

    private static readonly Action<ILogger, Guid, string, string, Exception?> ProductFoundAction =
        LoggerMessage.Define<Guid, string, string>(
            LogLevel.Debug,
            new EventId(9, "ProductFound"),
            "Product found: {ProductId} - {ProductName}, Category: {CategoryName}");

    private static readonly Action<ILogger, Guid, Exception?> ProductNotFoundAction =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(10, "ProductNotFound"),
            "Product not found: {ProductId}");

    private static readonly Action<ILogger, string, Exception?> GetProductBySlugStartedAction =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(11, "GetProductBySlugStarted"),
            "Getting product by slug: {Slug}");

    private static readonly Action<ILogger, string, Guid, string, Exception?> ProductFoundBySlugAction =
        LoggerMessage.Define<string, Guid, string>(
            LogLevel.Debug,
            new EventId(12, "ProductFoundBySlug"),
            "Product found with slug: {Slug} - {ProductId} - {ProductName}");

    private static readonly Action<ILogger, string, Exception?> ProductNotFoundBySlugAction =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(13, "ProductNotFoundBySlug"),
            "Product not found with slug: {Slug}");
    
    private static readonly Action<ILogger, int, int, string, string, Exception?> GetProductsListStartedAction =
        LoggerMessage.Define<int, int, string, string>(
            LogLevel.Information,
            new EventId(14, "GetProductsListStarted"),
            "Getting products list - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}, Category: {CategoryId}");

    private static readonly Action<ILogger, int, int, int, int, Exception?> GetProductsListCompletedAction =
        LoggerMessage.Define<int, int, int, int>(
            LogLevel.Information,
            new EventId(15, "GetProductsListCompleted"),
            "Products list retrieved - Page {PageNumber} of {TotalPages}, Items: {ItemCount}/{TotalCount}");

    private static readonly Action<ILogger, int, int, Exception?> ProductsFoundCountAction =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(16, "ProductsFoundCount"),
            "Found {ProductCount} products out of {TotalCount} total");
    
    private static readonly Action<ILogger, int, Exception?> GetFeaturedProductsStartedAction =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(17, "GetFeaturedProductsStarted"),
            "Getting featured products - Take: {Take}");

    private static readonly Action<ILogger, int, Exception?> FeaturedProductsFoundCountAction =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(18, "FeaturedProductsFoundCount"),
            "Found {Count} featured products");
    
    private static readonly Action<ILogger, string, Exception?> GetProductsOnSaleStartedAction =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(19, "GetProductsOnSaleStarted"),
            "Getting products on sale - Take: {Take}");

    private static readonly Action<ILogger, int, Exception?> ProductsOnSaleFoundCountAction =
        LoggerMessage.Define<int>(
            LogLevel.Debug,
            new EventId(20, "ProductsOnSaleFoundCount"),
            "Found {Count} products on sale");
    
    private static readonly Action<ILogger, Guid, int, Exception?> UpdateStockStartedAction =
        LoggerMessage.Define<Guid, int>(
            LogLevel.Information,
            new EventId(21, "UpdateStockStarted"),
            "Updating stock for product: {ProductId}, New Stock: {NewStock}");

    private static readonly Action<ILogger, Guid, int, int, Exception?> StockUpdatedAction =
        LoggerMessage.Define<Guid, int, int>(
            LogLevel.Information,
            new EventId(22, "StockUpdated"),
            "Stock updated successfully for product: {ProductId} - From {OldStock} to {NewStock}");
    
    private static readonly Action<ILogger, Guid, decimal, string, Exception?> UpdatePriceStartedAction =
        LoggerMessage.Define<Guid, decimal, string>(
            LogLevel.Information,
            new EventId(23, "UpdatePriceStarted"),
            "Updating price for product: {ProductId}, New Price: {NewPrice}, New Compare At Price: {CompareAtPrice}");

    private static readonly Action<ILogger, Guid, string, string, string, string, Exception?> PriceUpdatedAction =
        LoggerMessage.Define<Guid, string, string, string, string>(
            LogLevel.Information,
            new EventId(24, "PriceUpdated"),
            "Price updated successfully for product: {ProductId} - Price: From {OldPrice} to {NewPrice}, Compare At Price: From {OldCompareAt} to {NewCompareAt}");
    
    private static readonly Action<ILogger, string, int, Exception?> SearchProductsStartedAction =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(25, "SearchProductsStarted"),
            "Searching products with term: {SearchTerm}, Take: {Take}");

    private static readonly Action<ILogger, int, string, Exception?> SearchProductsFoundCountAction =
        LoggerMessage.Define<int, string>(
            LogLevel.Debug,
            new EventId(26, "SearchProductsFoundCount"),
            "Found {Count} products matching search term: {SearchTerm}");
    
    private static readonly Action<ILogger, int, Exception?> GetLowStockProductsStartedAction =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(27, "GetLowStockProductsStarted"),
            "Getting low stock products with threshold: {Threshold}");

    private static readonly Action<ILogger, int, int, Exception?> LowStockProductsFoundCountAction =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(28, "LowStockProductsFoundCount"),
            "Found {Count} products with stock below threshold {Threshold}");
    
    private static readonly Action<ILogger, Guid, int, Exception?> DecreaseStockStartedAction =
        LoggerMessage.Define<Guid, int>(
            LogLevel.Information,
            new EventId(29, "DecreaseStockStarted"),
            "Decreasing stock for product: {ProductId}, Quantity: {Quantity}");

    private static readonly Action<ILogger, Guid, int, int, Exception?> StockDecreasedAction =
        LoggerMessage.Define<Guid, int, int>(
            LogLevel.Information,
            new EventId(30, "StockDecreased"),
            "Stock decreased successfully for product: {ProductId} - From {OldStock} to {NewStock}");

    private static readonly Action<ILogger, Guid, string, int, Exception?> LowStockWarningAction =
        LoggerMessage.Define<Guid, string, int>(
            LogLevel.Warning,
            new EventId(31, "LowStockWarning"),
            "Product {ProductId} - {ProductName} is now low on stock: {Stock} units remaining");

    private static readonly Action<ILogger, Guid, string, Exception?> CannotDecreaseStockInactiveAction =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Warning,
            new EventId(32, "CannotDecreaseStockInactive"),
            "Cannot decrease stock for inactive product: {ProductId} - {ProductName}");

    private static readonly Action<ILogger, Guid, string, Exception?> CannotDecreaseStockDeletedAction =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Warning,
            new EventId(33, "CannotDecreaseStockDeleted"),
            "Cannot decrease stock for deleted product: {ProductId} - {ProductName}");

    private static readonly Action<ILogger, Guid, int, int, Exception?> InsufficientStockAction =
        LoggerMessage.Define<Guid, int, int>(
            LogLevel.Warning,
            new EventId(34, "InsufficientStock"),
            "Insufficient stock for product: {ProductId} - Current Stock: {CurrentStock}, Requested: {Requested}");
    
    private static readonly Action<ILogger, Guid, string, Exception?> GetProductsByCategoryStartedAction =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(35, "GetProductsByCategoryStarted"),
            "Getting products by category: {CategoryId}, Take: {Take}");

    private static readonly Action<ILogger, int, string, Exception?> ProductsByCategoryFoundCountAction =
        LoggerMessage.Define<int, string>(
            LogLevel.Debug,
            new EventId(36, "ProductsByCategoryFoundCount"),
            "Found {Count} products in category: {CategoryName}");

    private static readonly Action<ILogger, Guid, Exception?> CategoryNotFoundOrInactiveAction =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(37, "CategoryNotFoundOrInactive"),
            "Category not found or inactive: {CategoryId}");

    private static readonly Action<ILogger, int, int, Exception?> ProductsByCategoryLimitedAction =
        LoggerMessage.Define<int, int>(
            LogLevel.Debug,
            new EventId(38, "ProductsByCategoryLimited"),
            "Limited products from {OriginalCount} to {Take}");
    
    private static readonly Action<ILogger, Guid, Exception?> ActivateProductStartedAction =
    LoggerMessage.Define<Guid>(
        LogLevel.Information,
        new EventId(39, "ActivateProductStarted"),
        "Activating product: {ProductId}");

private static readonly Action<ILogger, Guid, string, Exception?> ProductActivatedAction =
    LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(40, "ProductActivated"),
        "Product activated successfully: {ProductId} - {ProductName}");

private static readonly Action<ILogger, Guid, string, Exception?> ProductAlreadyActiveAction =
    LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(41, "ProductAlreadyActive"),
        "Product already active: {ProductId} - {ProductName}");

private static readonly Action<ILogger, Guid, string, Exception?> CannotActivateDeletedProductAction =
    LoggerMessage.Define<Guid, string>(
        LogLevel.Warning,
        new EventId(42, "CannotActivateDeletedProduct"),
        "Cannot activate deleted product: {ProductId} - {ProductName}");

private static readonly Action<ILogger, Guid, Exception?> DeactivateProductStartedAction =
    LoggerMessage.Define<Guid>(
        LogLevel.Information,
        new EventId(43, "DeactivateProductStarted"),
        "Deactivating product: {ProductId}");

private static readonly Action<ILogger, Guid, string, Exception?> ProductDeactivatedAction =
    LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(44, "ProductDeactivated"),
        "Product deactivated successfully: {ProductId} - {ProductName}");

private static readonly Action<ILogger, Guid, string, Exception?> ProductAlreadyInactiveAction =
    LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(45, "ProductAlreadyInactive"),
        "Product already inactive: {ProductId} - {ProductName}");

private static readonly Action<ILogger, Guid, string, Exception?> CannotDeactivateDeletedProductAction =
    LoggerMessage.Define<Guid, string>(
        LogLevel.Warning,
        new EventId(46, "CannotDeactivateDeletedProduct"),
        "Cannot deactivate deleted product: {ProductId} - {ProductName}");

    // Public methods
    public static void ProductCreationStarted(this ILogger logger, string productName, Guid categoryId, decimal price, int stock)
    {
        ProductCreationStartedAction(logger, productName, categoryId, price, stock, null);
    }

    public static void ProductCreated(this ILogger logger, Guid productId, string productName)
    {
        ProductCreatedAction(logger, productId, productName, null);
    }

    public static void ProductUpdateStarted(this ILogger logger, Guid productId)
    {
        ProductUpdateStartedAction(logger, productId, null);
    }

    public static void ProductUpdated(this ILogger logger, Guid productId, string productName)
    {
        ProductUpdatedAction(logger, productId, productName, null);
    }

    public static void ProductDeleteStarted(this ILogger logger, Guid productId)
    {
        ProductDeleteStartedAction(logger, productId, null);
    }

    public static void ProductDeleted(this ILogger logger, Guid productId, string productName)
    {
        ProductDeletedAction(logger, productId, productName, null);
    }

    public static void ProductDeleteBlocked(this ILogger logger, Guid productId, int stock)
    {
        ProductDeleteBlockedAction(logger, productId, stock, null);
    }

    public static void GetProductByIdStarted(this ILogger logger, Guid productId)
    {
        GetProductByIdStartedAction(logger, productId, null);
    }

    public static void ProductFound(this ILogger logger, Guid productId, string productName, string categoryName)
    {
        ProductFoundAction(logger, productId, productName, categoryName, null);
    }

    public static void ProductNotFound(this ILogger logger, Guid productId)
    {
        ProductNotFoundAction(logger, productId, null);
    }

    public static void GetProductBySlugStarted(this ILogger logger, string slug)
    {
        GetProductBySlugStartedAction(logger, slug, null);
    }

    public static void ProductFoundBySlug(this ILogger logger, string slug, Guid productId, string productName)
    {
        ProductFoundBySlugAction(logger, slug, productId, productName, null);
    }

    public static void ProductNotFoundBySlug(this ILogger logger, string slug)
    {
        ProductNotFoundBySlugAction(logger, slug, null);
    }
    
    public static void GetProductsListStarted(this ILogger logger, int pageNumber, int pageSize, string searchTerm, string categoryId)
    {
        GetProductsListStartedAction(logger, pageNumber, pageSize, searchTerm, categoryId, null);
    }

    public static void GetProductsListCompleted(this ILogger logger, int pageNumber, int totalPages, int itemCount, int totalCount)
    {
        GetProductsListCompletedAction(logger, pageNumber, totalPages, itemCount, totalCount, null);
    }

    public static void ProductsFoundCount(this ILogger logger, int productCount, int totalCount)
    {
        ProductsFoundCountAction(logger, productCount, totalCount, null);
    }
    
    public static void GetFeaturedProductsStarted(this ILogger logger, int take)
    {
        GetFeaturedProductsStartedAction(logger, take, null);
    }

    public static void FeaturedProductsFoundCount(this ILogger logger, int count)
    {
        FeaturedProductsFoundCountAction(logger, count, null);
    }
    
    public static void GetProductsOnSaleStarted(this ILogger logger, string take)
    {
        GetProductsOnSaleStartedAction(logger, take, null);
    }

    public static void ProductsOnSaleFoundCount(this ILogger logger, int count)
    {
        ProductsOnSaleFoundCountAction(logger, count, null);
    }
    
    public static void UpdateStockStarted(this ILogger logger, Guid productId, int newStock)
    {
        UpdateStockStartedAction(logger, productId, newStock, null);
    }

    public static void StockUpdated(this ILogger logger, Guid productId, int oldStock, int newStock)
    {
        StockUpdatedAction(logger, productId, oldStock, newStock, null);
    }
    
    public static void UpdatePriceStarted(this ILogger logger, Guid productId, decimal newPrice, string compareAtPrice)
    {
        UpdatePriceStartedAction(logger, productId, newPrice, compareAtPrice, null);
    }

    public static void PriceUpdated(this ILogger logger, Guid productId, decimal oldPrice, decimal newPrice, decimal? oldCompareAt, decimal? newCompareAt)
    {
        PriceUpdatedAction(logger, 
            productId, 
            oldPrice.ToString("F2"), 
            newPrice.ToString("F2"), 
            oldCompareAt?.ToString("F2") ?? "none", 
            newCompareAt?.ToString("F2") ?? "none", 
            null);
    }
    
    public static void SearchProductsStarted(this ILogger logger, string searchTerm, int take)
    {
        SearchProductsStartedAction(logger, searchTerm, take, null);
    }

    public static void SearchProductsFoundCount(this ILogger logger, int count, string searchTerm)
    {
        SearchProductsFoundCountAction(logger, count, searchTerm, null);
    }
    
    public static void GetLowStockProductsStarted(this ILogger logger, int threshold)
    {
        GetLowStockProductsStartedAction(logger, threshold, null);
    }

    public static void LowStockProductsFoundCount(this ILogger logger, int count, int threshold)
    {
        LowStockProductsFoundCountAction(logger, count, threshold, null);
    }
    
    public static void DecreaseStockStarted(this ILogger logger, Guid productId, int quantity)
    {
        DecreaseStockStartedAction(logger, productId, quantity, null);
    }

    public static void StockDecreased(this ILogger logger, Guid productId, int oldStock, int newStock)
    {
        StockDecreasedAction(logger, productId, oldStock, newStock, null);
    }

    public static void LowStockWarning(this ILogger logger, Guid productId, string productName, int stock)
    {
        LowStockWarningAction(logger, productId, productName, stock, null);
    }

    public static void CannotDecreaseStockInactive(this ILogger logger, Guid productId, string productName)
    {
        CannotDecreaseStockInactiveAction(logger, productId, productName, null);
    }

    public static void CannotDecreaseStockDeleted(this ILogger logger, Guid productId, string productName)
    {
        CannotDecreaseStockDeletedAction(logger, productId, productName, null);
    }

    public static void InsufficientStock(this ILogger logger, Guid productId, int currentStock, int requested)
    {
        InsufficientStockAction(logger, productId, currentStock, requested, null);
    }
    
    public static void GetProductsByCategoryStarted(this ILogger logger, Guid categoryId, string take)
    {
        GetProductsByCategoryStartedAction(logger, categoryId, take, null);
    }

    public static void ProductsByCategoryFoundCount(this ILogger logger, int count, string categoryName)
    {
        ProductsByCategoryFoundCountAction(logger, count, categoryName, null);
    }

    public static void CategoryNotFoundOrInactive(this ILogger logger, Guid categoryId)
    {
        CategoryNotFoundOrInactiveAction(logger, categoryId, null);
    }

    public static void ProductsByCategoryLimited(this ILogger logger, int originalCount, int take)
    {
        ProductsByCategoryLimitedAction(logger, originalCount, take, null);
    }
    
    public static void ActivateProductStarted(this ILogger logger, Guid productId)
    {
        ActivateProductStartedAction(logger, productId, null);
    }

    public static void ProductActivated(this ILogger logger, Guid productId, string productName)
    {
        ProductActivatedAction(logger, productId, productName, null);
    }

    public static void ProductAlreadyActive(this ILogger logger, Guid productId, string productName)
    {
        ProductAlreadyActiveAction(logger, productId, productName, null);
    }

    public static void CannotActivateDeletedProduct(this ILogger logger, Guid productId, string productName)
    {
        CannotActivateDeletedProductAction(logger, productId, productName, null);
    }

    public static void DeactivateProductStarted(this ILogger logger, Guid productId)
    {
        DeactivateProductStartedAction(logger, productId, null);
    }

    public static void ProductDeactivated(this ILogger logger, Guid productId, string productName)
    {
        ProductDeactivatedAction(logger, productId, productName, null);
    }

    public static void ProductAlreadyInactive(this ILogger logger, Guid productId, string productName)
    {
        ProductAlreadyInactiveAction(logger, productId, productName, null);
    }

    public static void CannotDeactivateDeletedProduct(this ILogger logger, Guid productId, string productName)
    {
        CannotDeactivateDeletedProductAction(logger, productId, productName, null);
    }
}