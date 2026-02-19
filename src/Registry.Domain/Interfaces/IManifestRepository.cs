// <copyright file="IManifestRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Entities;
using Registry.Domain.Enums;

namespace Registry.Domain.Interfaces;

/// <summary>
/// Repository for <see cref="BinaryManifest"/> entities and their chunks.
/// </summary>
public interface IManifestRepository
{
    /// <summary>Get a manifest by ID with all chunks.</summary>
    Task<BinaryManifest?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Get the signed manifest for a specific product version, platform, and architecture.
    /// This is the endpoint the launcher calls before downloading chunks.
    /// </summary>
    Task<BinaryManifest?> GetForVersionAsync(
        Guid productId,
        Guid versionId,
        SupportedPlatform platform,
        CpuArchitecture arch,
        CancellationToken ct = default);

    /// <summary>Add a new manifest.</summary>
    void Add(BinaryManifest manifest);

    /// <summary>Persist all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
