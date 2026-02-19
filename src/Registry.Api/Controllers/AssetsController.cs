// <copyright file="AssetsController.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Registry.Application.DTOs;
using Registry.Application.Interfaces;
using Registry.Application.Services;
using Registry.Domain.Enums;

namespace Registry.Api.Controllers;

/// <summary>
/// Product asset management (icons, banners, screenshots).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly AssetService _assetService;
    private readonly ProductService _productService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AssetsController> _logger;

    /// <summary>Initialises a new instance.</summary>
    public AssetsController(
        AssetService assetService,
        ProductService productService,
        ICurrentUser currentUser,
        ILogger<AssetsController> logger)
    {
        _assetService = assetService;
        _productService = productService;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Initiate an asset upload — returns a presigned upload URL (15-minute TTL).
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Policy = "AssetWrite")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<ActionResult<AssetUploadDto>> InitiateUpload(
        [FromBody] InitiateAssetUploadRequest request, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(request.ProductId, ct);
        if (product is null) return NotFound();

        if (product.DeveloperId != _currentUser.UserId &&
            !_currentUser.HasPermission("admin.assets.write"))
        {
            return Forbid();
        }

        var upload = await _assetService.InitiateUploadAsync(
            request.ProductId, request.Type, request.ContentType,
            _currentUser.UserId!.Value, ct);

        _logger.LogInformation(
            "Asset upload initiated for product {ProductId}, type {Type} by {UserId}",
            request.ProductId, request.Type, _currentUser.UserId);

        return Ok(upload);
    }

    /// <summary>
    /// List assets for a product.
    /// </summary>
    [HttpGet("product/{productId:guid}")]
    [Authorize(Policy = "ProductRead")]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> ListByProduct(
        Guid productId, [FromQuery] AssetType? type, CancellationToken ct)
    {
        var assets = await _assetService.ListByProductAsync(productId, type, ct);
        return Ok(assets);
    }

    /// <summary>
    /// Delete an asset.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AssetWrite")]
    [EnableRateLimiting("WriteOperation")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _assetService.DeleteAsync(id, ct);

        _logger.LogInformation("Asset {AssetId} deleted by {UserId}", id, _currentUser.UserId);

        return NoContent();
    }
}

/// <summary>
/// Request body for initiating an asset upload.
/// </summary>
public sealed record InitiateAssetUploadRequest(
    Guid ProductId,
    AssetType Type,
    string FileName,
    string ContentType);
