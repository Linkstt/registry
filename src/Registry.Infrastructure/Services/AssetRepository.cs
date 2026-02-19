// <copyright file="AssetRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Registry.Domain.Entities;
using Registry.Domain.Enums;
using Registry.Domain.Interfaces;
using Registry.Infrastructure.Data;

namespace Registry.Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="IAssetRepository"/>.
/// </summary>
public sealed class AssetRepository : IAssetRepository
{
    private readonly RegistryDbContext _db;

    /// <summary>Initialises a new instance.</summary>
    public AssetRepository(RegistryDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<Asset?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Assets.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Asset>> ListByProductAsync(
        Guid productId, AssetType? type = null, CancellationToken ct = default)
    {
        var query = _db.Assets.Where(a => a.ProductId == productId);

        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        return await query.OrderBy(a => a.UploadedAt).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public void Add(Asset asset) => _db.Assets.Add(asset);

    /// <inheritdoc/>
    public void Delete(Asset asset) => _db.Assets.Remove(asset);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
