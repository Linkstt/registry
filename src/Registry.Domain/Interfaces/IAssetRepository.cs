// <copyright file="IAssetRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Entities;
using Registry.Domain.Enums;

namespace Registry.Domain.Interfaces;

/// <summary>
/// Repository for <see cref="Asset"/> entities.
/// </summary>
public interface IAssetRepository
{
    /// <summary>Get an asset by ID.</summary>
    Task<Asset?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>List assets for a product, optionally filtered by type.</summary>
    Task<IReadOnlyList<Asset>> ListByProductAsync(
        Guid productId, AssetType? type = null, CancellationToken ct = default);

    /// <summary>Add a new asset.</summary>
    void Add(Asset asset);

    /// <summary>Remove an asset.</summary>
    void Delete(Asset asset);

    /// <summary>Persist all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
