// <copyright file="VersionsController.cs" company="AllServices">
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
/// Version management for products.
/// </summary>
[ApiController]
[Route("api/products/{productId:guid}/versions")]
public class VersionsController : ControllerBase
{
    private readonly VersionService _versionService;
    private readonly ProductService _productService;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<CreateVersionRequest> _createValidator;
    private readonly ILogger<VersionsController> _logger;

    /// <summary>Initialises a new instance.</summary>
    public VersionsController(
        VersionService versionService,
        ProductService productService,
        ICurrentUser currentUser,
        IValidator<CreateVersionRequest> createValidator,
        ILogger<VersionsController> logger)
    {
        _versionService = versionService;
        _productService = productService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _logger = logger;
    }

    /// <summary>
    /// List versions for a product.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "VersionRead")]
    public async Task<ActionResult<IReadOnlyList<VersionSummaryDto>>> List(
        Guid productId,
        [FromQuery] VersionStatus? status,
        [FromQuery] ReleaseChannel? channel,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var versions = await _versionService.ListByProductAsync(productId, status, channel, page, pageSize, ct);
        return Ok(versions);
    }

    /// <summary>
    /// Get a specific version.
    /// </summary>
    [HttpGet("{versionId:guid}")]
    [Authorize(Policy = "VersionRead")]
    public async Task<ActionResult<VersionDetailDto>> GetById(
        Guid productId, Guid versionId, CancellationToken ct)
    {
        var version = await _versionService.GetByIdAsync(versionId, ct);
        if (version is null || version.ProductId != productId) return NotFound();
        return Ok(version);
    }

    /// <summary>
    /// Create a new version for a product.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "VersionWrite")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<ActionResult<VersionDetailDto>> Create(
        Guid productId, [FromBody] CreateVersionRequest request, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAsync(request, ct);

        // Verify the caller owns this product
        var product = await _productService.GetByIdAsync(productId, ct);
        if (product is null) return NotFound();

        if (product.DeveloperId != _currentUser.UserId &&
            !_currentUser.HasPermission("admin.versions.write"))
        {
            return Forbid();
        }

        var version = await _versionService.CreateAsync(productId, request, ct);

        _logger.LogInformation(
            "Version {VersionString} created for product {ProductId} by {UserId}",
            version.VersionString, productId, _currentUser.UserId);

        return CreatedAtAction(nameof(GetById), new { productId, versionId = version.Id }, version);
    }

    /// <summary>
    /// Yank a version (developer or admin action).
    /// </summary>
    [HttpPost("{versionId:guid}/yank")]
    [Authorize(Policy = "VersionWrite")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<IActionResult> Yank(
        Guid productId, Guid versionId, [FromBody] YankVersionRequest request, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(productId, ct);
        if (product is null) return NotFound();

        if (product.DeveloperId != _currentUser.UserId &&
            !_currentUser.HasPermission("admin.versions.yank"))
        {
            return Forbid();
        }

        await _versionService.YankAsync(versionId, ct);

        _logger.LogInformation(
            "Version {VersionId} yanked for product {ProductId} by {UserId}. Reason: {Reason}",
            versionId, productId, _currentUser.UserId, request.Reason ?? "none");

        return NoContent();
    }

    /// <summary>
    /// Transition a version's status (admin / CI pipeline).
    /// </summary>
    [HttpPost("{versionId:guid}/status")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<IActionResult> TransitionStatus(
        Guid productId, Guid versionId,
        [FromBody] StatusTransitionRequest request, CancellationToken ct)
    {
        var version = await _versionService.GetByIdAsync(versionId, ct);
        if (version is null || version.ProductId != productId) return NotFound();

        await _versionService.TransitionStatusAsync(versionId, request.NewStatus, ct);

        _logger.LogInformation(
            "Version {VersionId} transitioned to {Status} by {UserId}",
            versionId, request.NewStatus, _currentUser.UserId);

        return NoContent();
    }
}

/// <summary>
/// Request body for version status transitions.
/// </summary>
public sealed record StatusTransitionRequest(VersionStatus NewStatus);
