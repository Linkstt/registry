// <copyright file="VersionService.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Application.DTOs;
using Registry.Domain.Entities;
using Registry.Domain.Enums;
using Registry.Domain.Interfaces;

namespace Registry.Application.Services;

/// <summary>
/// Product version lifecycle — create, list, yank.
/// </summary>
public sealed class VersionService
{
    private readonly IVersionRepository _versions;
    private readonly IProductRepository _products;

    /// <summary>Initialises a new instance.</summary>
    public VersionService(IVersionRepository versions, IProductRepository products)
    {
        _versions = versions;
        _products = products;
    }

    /// <summary>
    /// Initiate a new version for a product. Returns the version in Uploading status.
    /// Callers then upload per-platform binaries via the asset/chunk endpoints.
    /// </summary>
    public async Task<VersionDetailDto> CreateAsync(
        Guid productId, CreateVersionRequest request, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        // Prevent duplicate version strings within the same product
        var existing = await _versions.GetByProductAndVersionAsync(productId, request.VersionString, ct);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Version '{request.VersionString}' already exists for product {productId}.");
        }

        var version = new ProductVersion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            VersionString = request.VersionString,
            Channel = request.Channel,
            Changelog = request.Changelog,
            ReleaseNotes = request.ReleaseNotes,
            Source = request.Source,
            CiJobId = request.CiJobId,
            Status = VersionStatus.Uploading,
            IsForcedUpdate = request.IsForcedUpdate,
            RolloutPercentage = Math.Clamp(request.RolloutPercentage, 0, 100),
            MinimumLauncherVersion = request.MinimumLauncherVersion,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _versions.Add(version);
        await _versions.SaveChangesAsync(ct);

        return MapToDetail(version);
    }

    /// <summary>Get a version by ID.</summary>
    public async Task<VersionDetailDto?> GetByIdAsync(Guid versionId, CancellationToken ct = default)
    {
        var version = await _versions.GetByIdAsync(versionId, ct);
        return version is null ? null : MapToDetail(version);
    }

    /// <summary>List versions for a product.</summary>
    public async Task<IReadOnlyList<VersionSummaryDto>> ListByProductAsync(
        Guid productId,
        VersionStatus? status = null,
        ReleaseChannel? channel = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var versions = await _versions.ListByProductAsync(productId, status, channel, page, pageSize, ct);
        return versions.Select(MapToSummary).ToList();
    }

    /// <summary>Yank a version — pulls it from distribution but preserves the record.</summary>
    public async Task YankAsync(Guid versionId, CancellationToken ct = default)
    {
        var version = await _versions.GetByIdAsync(versionId, ct)
            ?? throw new KeyNotFoundException($"Version {versionId} not found.");

        if (version.Status == VersionStatus.Yanked)
        {
            throw new InvalidOperationException("Version is already yanked.");
        }

        version.Status = VersionStatus.Yanked;
        version.YankedAt = DateTime.UtcNow;
        version.UpdatedAt = DateTime.UtcNow;

        _versions.Update(version);
        await _versions.SaveChangesAsync(ct);
    }

    /// <summary>Transition version status through the review pipeline.</summary>
    public async Task TransitionStatusAsync(
        Guid versionId, VersionStatus newStatus, CancellationToken ct = default)
    {
        var version = await _versions.GetByIdAsync(versionId, ct)
            ?? throw new KeyNotFoundException($"Version {versionId} not found.");

        ValidateTransition(version.Status, newStatus);

        version.Status = newStatus;
        version.UpdatedAt = DateTime.UtcNow;

        if (newStatus == VersionStatus.Approved)
        {
            version.ApprovedAt = DateTime.UtcNow;
        }

        _versions.Update(version);
        await _versions.SaveChangesAsync(ct);
    }

    // ──── Pipeline transition validation ────

    private static void ValidateTransition(VersionStatus current, VersionStatus next)
    {
        var allowed = current switch
        {
            VersionStatus.Uploading => new[] { VersionStatus.Processing },
            VersionStatus.Processing => new[] { VersionStatus.ScanPending },
            VersionStatus.ScanPending => new[] { VersionStatus.ScanFailed, VersionStatus.ReviewPending },
            VersionStatus.ScanFailed => new[] { VersionStatus.ScanPending }, // rescan
            VersionStatus.ReviewPending => new[] { VersionStatus.Approved, VersionStatus.Rejected },
            VersionStatus.Rejected => new[] { VersionStatus.Uploading }, // resubmit
            VersionStatus.Approved => new[] { VersionStatus.Yanked },
            VersionStatus.Yanked => Array.Empty<VersionStatus>(),
            _ => Array.Empty<VersionStatus>(),
        };

        if (!allowed.Contains(next))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {current} to {next}.");
        }
    }

    // ──── Mapping ────

    private static VersionDetailDto MapToDetail(ProductVersion v) => new(
        v.Id, v.ProductId, v.VersionString, v.Channel,
        v.Changelog, v.ReleaseNotes, v.Source, v.CiJobId,
        v.Status, v.IsForcedUpdate, v.RolloutPercentage,
        v.MinimumLauncherVersion,
        v.UploadedAt, v.ApprovedAt, v.YankedAt,
        v.PlatformBinaries.Select(pb => new PlatformBinaryDto(
            pb.Id, pb.Platform, pb.Arch, pb.SizeBytes, pb.ManifestId)).ToList());

    private static VersionSummaryDto MapToSummary(ProductVersion v) => new(
        v.Id, v.ProductId, v.VersionString, v.Channel,
        v.Source, v.Status, v.RolloutPercentage,
        v.UploadedAt, v.ApprovedAt, v.YankedAt);
}
