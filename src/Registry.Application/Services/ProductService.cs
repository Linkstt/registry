// <copyright file="ProductService.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Application.DTOs;
using Registry.Application.Interfaces;
using Registry.Domain.Entities;
using Registry.Domain.Enums;
using Registry.Domain.Interfaces;

namespace Registry.Application.Services;

/// <summary>
/// Product lifecycle management — create, update, list, change status.
/// </summary>
public sealed class ProductService
{
    private readonly IProductRepository _products;
    private readonly IObjectStorage _storage;

    /// <summary>Initialises a new instance.</summary>
    public ProductService(IProductRepository products, IObjectStorage storage)
    {
        _products = products;
        _storage = storage;
    }

    /// <summary>Create a new product in Draft status.</summary>
    public async Task<ProductDetailDto> CreateAsync(
        Guid developerId, CreateProductRequest request, CancellationToken ct = default)
    {
        if (await _products.SlugExistsAsync(request.Slug, ct: ct))
        {
            throw new InvalidOperationException($"Slug '{request.Slug}' is already taken.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            DeveloperId = developerId,
            Name = request.Name,
            Slug = request.Slug.ToLowerInvariant(),
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            CategoryId = request.CategoryId,
            Tags = request.Tags ?? [],
            Status = ProductStatus.Draft,
            Visibility = request.Visibility,
            PlatformSupport = request.PlatformSupport,
            TrailerUrl = request.TrailerUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _products.Add(product);
        await _products.SaveChangesAsync(ct);

        return MapToDetail(product);
    }

    /// <summary>Get a product by ID.</summary>
    public async Task<ProductDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(id, ct);
        return product is null ? null : MapToDetail(product);
    }

    /// <summary>Get a product by slug.</summary>
    public async Task<ProductDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await _products.GetBySlugAsync(slug, ct);
        return product is null ? null : MapToDetail(product);
    }

    /// <summary>List products with pagination and filtering.</summary>
    public async Task<PagedResult<ProductSummaryDto>> ListAsync(
        Guid? developerId = null,
        ProductStatus? status = null,
        Guid? categoryId = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _products.ListAsync(
            developerId, status, categoryId, searchTerm, page, pageSize, ct);

        var dtos = items.Select(MapToSummary).ToList();
        return new PagedResult<ProductSummaryDto>(dtos, totalCount, page, pageSize);
    }

    /// <summary>Update product fields. Only non-null fields are applied.</summary>
    public async Task<ProductDetailDto> UpdateAsync(
        Guid productId, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        if (request.Name is not null) product.Name = request.Name;
        if (request.ShortDescription is not null) product.ShortDescription = request.ShortDescription;
        if (request.LongDescription is not null) product.LongDescription = request.LongDescription;
        if (request.CategoryId is not null) product.CategoryId = request.CategoryId;
        if (request.Tags is not null) product.Tags = request.Tags;
        if (request.Visibility is not null) product.Visibility = request.Visibility.Value;
        if (request.PlatformSupport is not null) product.PlatformSupport = request.PlatformSupport;
        if (request.TrailerUrl is not null) product.TrailerUrl = request.TrailerUrl;

        product.UpdatedAt = DateTime.UtcNow;

        _products.Update(product);
        await _products.SaveChangesAsync(ct);

        return MapToDetail(product);
    }

    /// <summary>Soft-delete: only allowed if no active licenses exist (enforced at API level).</summary>
    public async Task DeleteAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        product.Status = ProductStatus.Delisted;
        product.UpdatedAt = DateTime.UtcNow;

        _products.Update(product);
        await _products.SaveChangesAsync(ct);
    }

    /// <summary>Suspend a product (admin action).</summary>
    public async Task SuspendAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        product.Status = ProductStatus.Suspended;
        product.UpdatedAt = DateTime.UtcNow;

        _products.Update(product);
        await _products.SaveChangesAsync(ct);
    }

    /// <summary>Restore a suspended product.</summary>
    public async Task UnsuspendAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        if (product.Status != ProductStatus.Suspended)
        {
            throw new InvalidOperationException("Product is not suspended.");
        }

        product.Status = ProductStatus.Listed;
        product.UpdatedAt = DateTime.UtcNow;

        _products.Update(product);
        await _products.SaveChangesAsync(ct);
    }

    // ──── Mapping ────

    private ProductDetailDto MapToDetail(Product p) => new(
        p.Id, p.DeveloperId, p.Name, p.Slug,
        p.ShortDescription, p.LongDescription,
        p.CategoryId, p.Category?.Name,
        p.Tags, p.Status, p.Visibility,
        p.PlatformSupport, p.TrustBadge,
        ResolveAssetUrl(p.IconAssetId),
        ResolveAssetUrl(p.BannerAssetId),
        p.ScreenshotAssetIds.Select(id => ResolveAssetUrl(id) ?? string.Empty).Where(u => u.Length > 0).ToList(),
        p.TrailerUrl,
        p.CreatedAt, p.UpdatedAt, p.PublishedAt);

    private ProductSummaryDto MapToSummary(Product p) => new(
        p.Id, p.DeveloperId, p.Name, p.Slug,
        p.ShortDescription, p.Status, p.Visibility,
        p.PlatformSupport, p.TrustBadge,
        ResolveAssetUrl(p.IconAssetId),
        p.CreatedAt, p.PublishedAt);

    private string? ResolveAssetUrl(Guid? assetId)
    {
        if (assetId is null) return null;
        return _storage.GetPublicUrl("assets", $"products/{assetId}");
    }
}
