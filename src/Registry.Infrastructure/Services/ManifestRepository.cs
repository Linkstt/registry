// <copyright file="ManifestRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Registry.Domain.Entities;
using Registry.Domain.Enums;
using Registry.Domain.Interfaces;
using Registry.Infrastructure.Data;

namespace Registry.Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="IManifestRepository"/>.
/// </summary>
public sealed class ManifestRepository : IManifestRepository
{
    private readonly RegistryDbContext _db;

    /// <summary>Initialises a new instance.</summary>
    public ManifestRepository(RegistryDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<BinaryManifest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.BinaryManifests
            .Include(m => m.Chunks.OrderBy(c => c.SequenceIndex))
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<BinaryManifest?> GetForVersionAsync(
        Guid productId,
        Guid versionId,
        SupportedPlatform platform,
        CpuArchitecture arch,
        CancellationToken ct = default)
    {
        return await _db.PlatformBinaries
            .Where(pb => pb.VersionId == versionId
                && pb.Platform == platform
                && pb.Arch == arch
                && pb.Version.ProductId == productId
                && pb.Version.Status == VersionStatus.Approved)
            .Select(pb => pb.Manifest)
            .Include(m => m.Chunks.OrderBy(c => c.SequenceIndex))
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public void Add(BinaryManifest manifest) => _db.BinaryManifests.Add(manifest);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
