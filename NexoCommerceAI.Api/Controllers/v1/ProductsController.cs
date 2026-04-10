using MediatR;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Api.Controllers.v1;

/// <summary>
/// Controller for managing products in the e-commerce system.
/// Provides endpoints for CRUD operations, stock management, pricing, and product queries.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiVersion("1.0")]
public class ProductsController(IMediator mediator, ILogger<ProductsController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly ILogger<ProductsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    #region Queries

    /// <summary>
    /// Gets a paginated list of products with optional filtering and sorting.
    /// </summary>
    /// <param name="pagination">Pagination parameters including page number, page size, search term, filters, and sorting options.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A paginated result containing product responses.</returns>
    /// <response code="200">Returns the paginated list of products.</response>
    /// <response code="400">If the pagination parameters are invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<ProductResponse>>> GetProducts(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var query = new GetProductsListQuery { Pagination = pagination };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The product if found; otherwise, a 404 status code.</returns>
    /// <response code="200">Returns the requested product.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProductById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets a product by its SEO-friendly slug.
    /// </summary>
    /// <param name="slug">The SEO-friendly slug of the product.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The product if found; otherwise, a 404 status code.</returns>
    /// <response code="200">Returns the requested product.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProductBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var query = new GetProductBySlugQuery(slug);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets a list of featured products.
    /// </summary>
    /// <param name="take">The maximum number of products to return (default is 10, max is 100).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of featured products.</returns>
    /// <response code="200">Returns the list of featured products.</response>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetFeaturedProducts(
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFeaturedProductsQuery(take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a list of products currently on sale.
    /// </summary>
    /// <param name="take">The maximum number of products to return (optional).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of products on sale.</returns>
    /// <response code="200">Returns the list of products on sale.</response>
    [HttpGet("on-sale")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProductsOnSale(
        [FromQuery] int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsOnSaleQuery(take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Searches for products by name, description, or SKU.
    /// </summary>
    /// <param name="searchTerm">The search term to look for.</param>
    /// <param name="take">The maximum number of products to return (default is 20, max is 100).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of products matching the search term.</returns>
    /// <response code="200">Returns the search results.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> SearchProducts(
        [FromQuery] string searchTerm,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchProductsQuery(searchTerm, take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets products with low stock levels.
    /// </summary>
    /// <param name="threshold">The stock threshold to consider as low (default is 5, max is 100).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of products with stock below the threshold.</returns>
    /// <response code="200">Returns the list of low stock products.</response>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetLowStockProducts(
        [FromQuery] int threshold = 5,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLowStockProductsQuery(threshold);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets products by category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category.</param>
    /// <param name="take">The maximum number of products to return (optional).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of products in the specified category.</returns>
    /// <response code="200">Returns the list of products.</response>
    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProductsByCategory(
        Guid categoryId,
        [FromQuery] int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsByCategoryQuery(categoryId, take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets related products based on category.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="take">The maximum number of related products to return (default is 5, max is 20).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of related products.</returns>
    /// <response code="200">Returns the list of related products.</response>
    [HttpGet("{productId:guid}/related")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetRelatedProducts(
        Guid productId,
        [FromQuery] int take = 5,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRelatedProductsQuery(productId, take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets total stock value and inventory statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Stock value statistics including total units, total value, and average price.</returns>
    /// <response code="200">Returns the stock value statistics.</response>
    [HttpGet("stock-value")]
    [ProducesResponseType(typeof(TotalStockValueResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TotalStockValueResponse>> GetTotalStockValue(
        CancellationToken cancellationToken = default)
    {
        var query = new GetTotalStockValueQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets comprehensive product statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Product statistics including totals, stock metrics, pricing, and category breakdowns.</returns>
    /// <response code="200">Returns the product statistics.</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ProductsStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductsStatisticsResponse>> GetProductsStatistics(
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsStatisticsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="command">The create product command containing product details.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The created product.</returns>
    /// <response code="201">Returns the newly created product.</response>
    /// <response code="400">If the product data is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="command">The update product command containing the fields to update.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The updated product.</returns>
    /// <response code="200">Returns the updated product.</response>
    /// <response code="400">If the product data is invalid or ID mismatch.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in the URL does not match the ID in the request body.");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes (soft deletes) a product.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the product was successfully deleted.</response>
    /// <response code="400">If the product cannot be deleted (e.g., has stock).</response>
    /// <response code="404">If the product is not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Activates a product (makes it available for sale).
    /// </summary>
    /// <param name="id">The unique identifier of the product to activate.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the product was successfully activated.</response>
    /// <response code="400">If the product cannot be activated.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateProductCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Deactivates a product (makes it unavailable for sale).
    /// </summary>
    /// <param name="id">The unique identifier of the product to deactivate.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the product was successfully deactivated.</response>
    /// <response code="400">If the product cannot be deactivated.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateProductCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Updates the stock quantity of a product.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="newStock">The new stock quantity.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the stock was successfully updated.</response>
    /// <response code="400">If the stock value is invalid.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateStock(
        Guid id,
        [FromBody] int newStock,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductStockCommand { ProductId = id, NewStock = newStock };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Decreases the stock quantity of a product (e.g., after a purchase).
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="quantity">The quantity to decrease by.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the stock was successfully decreased.</response>
    /// <response code="400">If the quantity is invalid or insufficient stock.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPatch("{id:guid}/decrease-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DecreaseStock(
        Guid id,
        [FromBody] int quantity,
        CancellationToken cancellationToken)
    {
        var command = new DecreaseProductStockCommand(id, quantity);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Updates the price of a product.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="command">The update price command containing new prices.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the price was successfully updated.</response>
    /// <response code="400">If the price values are invalid.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPatch("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePrice(
        Guid id,
        [FromBody] UpdateProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.ProductId)
        {
            return BadRequest("ID in the URL does not match the ID in the request body.");
        }

        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Performs a bulk update of stock quantities for multiple products.
    /// </summary>
    /// <param name="command">The bulk update command containing a list of product stock updates.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The result of the bulk update operation.</returns>
    /// <response code="200">Returns the bulk update result.</response>
    /// <response code="400">If the bulk update data is invalid.</response>
    [HttpPatch("bulk-stock")]
    [ProducesResponseType(typeof(BulkUpdateStockResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkUpdateStockResult>> BulkUpdateStock(
        [FromBody] BulkUpdateStockCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    #endregion
}