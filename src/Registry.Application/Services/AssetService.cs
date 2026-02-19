// <copyright file="AssetService.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Application.DTOs;
using Registry.Application.Interfaces;
using Registry.Domain.Entities;
using Registry.Domain.Enums;
using Registry.Domain.Interfaces;

namespace Registry.Application.Services;

/// <summary>
/// Upload and manage product assets (icons, banners, screenshots).
/// </summary>
public sealed class AssetService
{
    private readonly IAssetRepository _assets;
    private readonly IObjectStorage _storage;

    private static readonly TimeSpan UploadUrlExpiry = TimeSpan.FromMinutes(15);

    /// <summary>Initialises a new instance.</summary>
    public AssetService(IAssetRepository assets, IObjectStorage storage)
    {
        _assets = assets;
        _storage = storage;
    }

    /// <summary>
    /// Initiate an asset upload — returns a presigned URL for the client to PUT to.
    /// </summary>
    public async Task<AssetUploadDto> InitiateUploadAsync(
        Guid productId,
        AssetType type,
        string contentType,
        Guid uploadedBy,
        CancellationToken ct = default)
    {
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Type = type,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var objectKey = $"products/{productId}/{type.ToString().ToLowerInvariant()}/{asset.Id}";
        asset.CdnPath = objectKey;

        var uploadUrl = await _storage.GeneratePresignedUploadUrlAsync(
            "assets", objectKey, contentType, UploadUrlExpiry, ct);

        var cdnUrl = _storage.GetPublicUrl("assets", objectKey);

        _assets.Add(asset);
        await _assets.SaveChangesAsync(ct);

        return new AssetUploadDto(asset.Id, uploadUrl, cdnUrl);
    }

    /// <summary>List assets for a product.</summary>
    public async Task<IReadOnlyList<AssetDto>> ListByProductAsync(
        Guid productId, AssetType? type = null, CancellationToken ct = default)
    {
        var assets = await _assets.ListByProductAsync(productId, type, ct);

        return assets.Select(a => new AssetDto(
            a.Id, a.ProductId, a.Type,
            _storage.GetPublicUrl("assets", a.CdnPath),
            a.Width, a.Height, a.SizeBytes,
            a.UploadedAt)).ToList();
    }

    /// <summary>Delete an asset.</summary>
    public async Task DeleteAsync(Guid assetId, CancellationToken ct = default)
    {
        var asset = await _assets.GetByIdAsync(assetId, ct)
            ?? throw new KeyNotFoundException($"Asset {assetId} not found.");

        _assets.Delete(asset);
        await _assets.SaveChangesAsync(ct);
    }
}
