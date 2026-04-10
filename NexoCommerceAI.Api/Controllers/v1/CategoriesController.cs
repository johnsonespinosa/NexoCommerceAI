using MediatR;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Application.Features.Categories.Queries;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Api.Controllers.v1;

/// <summary>
/// Controller for managing product categories in the e-commerce system.
/// Provides endpoints for CRUD operations, activation/deactivation, and category queries.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiVersion("1.0")]
public class CategoriesController(IMediator mediator, ILogger<CategoriesController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly ILogger<CategoriesController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    #region Queries

    /// <summary>
    /// Gets a paginated list of categories with optional filtering and sorting.
    /// </summary>
    /// <param name="pagination">Pagination parameters including page number, page size, search term, and sorting options.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A paginated result containing category responses.</returns>
    /// <response code="200">Returns the paginated list of categories.</response>
    /// <response code="400">If the pagination parameters are invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<CategoryResponse>>> GetCategories(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoriesListQuery { Pagination = pagination };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a category by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the category.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The category if found; otherwise, a 404 status code.</returns>
    /// <response code="200">Returns the requested category.</response>
    /// <response code="404">If the category is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets a category by its SEO-friendly slug.
    /// </summary>
    /// <param name="slug">The SEO-friendly slug of the category.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The category if found; otherwise, a 404 status code.</returns>
    /// <response code="200">Returns the requested category.</response>
    /// <response code="404">If the category is not found.</response>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetCategoryBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var query = new GetCategoryBySlugQuery(slug);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all products belonging to a specific category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category.</param>
    /// <param name="take">The maximum number of products to return (optional).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A list of products in the specified category.</returns>
    /// <response code="200">Returns the list of products.</response>
    /// <response code="404">If the category is not found.</response>
    [HttpGet("{categoryId:guid}/products")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetCategoryProducts(
        Guid categoryId,
        [FromQuery] int? take = null,
        CancellationToken cancellationToken = default)
    {
        // Primero verificar que la categoría existe
        var categoryQuery = new GetCategoryByIdQuery(categoryId);
        var category = await _mediator.Send(categoryQuery, cancellationToken);
        
        if (category == null)
        {
            return NotFound($"Category with ID '{categoryId}' was not found.");
        }
        
        var query = new GetProductsByCategoryQuery(categoryId, take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="command">The create category command containing category details.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The created category.</returns>
    /// <response code="201">Returns the newly created category.</response>
    /// <response code="400">If the category data is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The unique identifier of the category to update.</param>
    /// <param name="command">The update category command containing the fields to update.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The updated category.</returns>
    /// <response code="200">Returns the updated category.</response>
    /// <response code="400">If the category data is invalid or ID mismatch.</response>
    /// <response code="404">If the category is not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryCommand command,
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
    /// Deletes (soft deletes) a category.
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the category was successfully deleted.</response>
    /// <response code="400">If the category cannot be deleted (e.g., has products).</response>
    /// <response code="404">If the category is not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Activates a category (makes it available for product assignment).
    /// </summary>
    /// <param name="id">The unique identifier of the category to activate.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the category was successfully activated.</response>
    /// <response code="400">If the category cannot be activated.</response>
    /// <response code="404">If the category is not found.</response>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateCategoryCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Deactivates a category (makes it unavailable for product assignment).
    /// </summary>
    /// <param name="id">The unique identifier of the category to deactivate.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the category was successfully deactivated.</response>
    /// <response code="400">If the category cannot be deactivated.</response>
    /// <response code="404">If the category is not found.</response>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateCategoryCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    #endregion
}