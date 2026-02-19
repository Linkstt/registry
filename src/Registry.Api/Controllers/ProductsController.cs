// <copyright file="ProductsController.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Registry.Application.DTOs;
using Registry.Application.Interfaces;
using Registry.Application.Services;
using Registry.Domain.Enums;

namespace Registry.Api.Controllers;

/// <summary>
/// Product CRUD and lifecycle management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>Initialises a new instance.</summary>
    public ProductsController(
        ProductService productService,
        ICurrentUser currentUser,
        IValidator<CreateProductRequest> createValidator,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _logger = logger;
    }

    /// <summary>
    /// List products with pagination and filtering.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ProductRead")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> List(
        [FromQuery] Guid? developerId,
        [FromQuery] ProductStatus? status,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _productService.ListAsync(developerId, status, categoryId, q, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ProductRead")]
    public async Task<ActionResult<ProductDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Get a product by slug.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [Authorize(Policy = "ProductRead")]
    public async Task<ActionResult<ProductDetailDto>> GetBySlug(string slug, CancellationToken ct)
    {
        var product = await _productService.GetBySlugAsync(slug, ct);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ProductWrite")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<ActionResult<ProductDetailDto>> Create(
        [FromBody] CreateProductRequest request, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAsync(request, ct);

        var developerId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User identity not available.");

        var product = await _productService.CreateAsync(developerId, request, ct);

        _logger.LogInformation(
            "Product {ProductId} created by developer {DeveloperId}",
            product.Id, developerId);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Update a product.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "ProductWrite")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<ActionResult<ProductDetailDto>> Update(
        Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        // Developers can only update their own products
        var existing = await _productService.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        if (existing.DeveloperId != _currentUser.UserId &&
            !_currentUser.HasPermission("admin.products.write"))
        {
            return Forbid();
        }

        var product = await _productService.UpdateAsync(id, request, ct);
        return Ok(product);
    }

    /// <summary>
    /// Soft-delete a product (sets status to Delisted).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ProductWrite")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var existing = await _productService.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        if (existing.DeveloperId != _currentUser.UserId &&
            !_currentUser.HasPermission("admin.products.delist"))
        {
            return Forbid();
        }

        await _productService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Suspend a product (admin action).
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        await _productService.SuspendAsync(id, ct);

        _logger.LogInformation(
            "Product {ProductId} suspended by {UserId}",
            id, _currentUser.UserId);

        return NoContent();
    }

    /// <summary>
    /// Unsuspend a product (admin action).
    /// </summary>
    [HttpPost("{id:guid}/unsuspend")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<IActionResult> Unsuspend(Guid id, CancellationToken ct)
    {
        await _productService.UnsuspendAsync(id, ct);

        _logger.LogInformation(
            "Product {ProductId} unsuspended by {UserId}",
            id, _currentUser.UserId);

        return NoContent();
    }
}
