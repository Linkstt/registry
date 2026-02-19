// <copyright file="VersionRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Registry.Domain.Entities;
using Registry.Domain.Enums;
using Registry.Domain.Interfaces;
using Registry.Infrastructure.Data;

namespace Registry.Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="IVersionRepository"/>.
/// </summary>
public sealed class VersionRepository : IVersionRepository
{
    private readonly RegistryDbContext _db;

    /// <summary>Initialises a new instance.</summary>
    public VersionRepository(RegistryDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<ProductVersion?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.ProductVersions
            .Include(v => v.PlatformBinaries)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<ProductVersion?> GetByProductAndVersionAsync(
        Guid productId, string versionString, CancellationToken ct = default)
    {
        return await _db.ProductVersions
            .FirstOrDefaultAsync(v => v.ProductId == productId && v.VersionString == versionString, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductVersion>> ListByProductAsync(
        Guid productId,
        VersionStatus? status = null,
        ReleaseChannel? channel = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _db.ProductVersions
            .Include(v => v.PlatformBinaries)
            .Where(v => v.ProductId == productId);

        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);

        if (channel.HasValue)
            query = query.Where(v => v.Channel == channel.Value);

        return await query
            .OrderByDescending(v => v.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ProductVersion?> GetLatestApprovedAsync(
        Guid productId, ReleaseChannel channel = ReleaseChannel.Stable, CancellationToken ct = default)
    {
        return await _db.ProductVersions
            .Include(v => v.PlatformBinaries)
            .Where(v => v.ProductId == productId
                && v.Status == VersionStatus.Approved
                && v.Channel == channel)
            .OrderByDescending(v => v.ApprovedAt)
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public void Add(ProductVersion version) => _db.ProductVersions.Add(version);

    /// <inheritdoc/>
    public void Update(ProductVersion version) => _db.ProductVersions.Update(version);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
