// <copyright file="IVersionRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Entities;
using Registry.Domain.Enums;

namespace Registry.Domain.Interfaces;

/// <summary>
/// Repository for <see cref="ProductVersion"/> entities.
/// </summary>
public interface IVersionRepository
{
    /// <summary>Get a version by ID, including platform binaries.</summary>
    Task<ProductVersion?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Get a version by product ID and version string.</summary>
    Task<ProductVersion?> GetByProductAndVersionAsync(
        Guid productId, string versionString, CancellationToken ct = default);

    /// <summary>List versions for a product, ordered by upload date descending.</summary>
    Task<IReadOnlyList<ProductVersion>> ListByProductAsync(
        Guid productId,
        VersionStatus? status = null,
        ReleaseChannel? channel = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>Get the latest approved version for a product on a given channel.</summary>
    Task<ProductVersion?> GetLatestApprovedAsync(
        Guid productId,
        ReleaseChannel channel = ReleaseChannel.Stable,
        CancellationToken ct = default);

    /// <summary>Add a new version.</summary>
    void Add(ProductVersion version);

    /// <summary>Mark a version as modified.</summary>
    void Update(ProductVersion version);

    /// <summary>Persist all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
