// <copyright file="ManifestService.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Application.DTOs;
using Registry.Application.Interfaces;
using Registry.Domain.Enums;
using Registry.Domain.Interfaces;

namespace Registry.Application.Services;

/// <summary>
/// Retrieves signed manifests for the launcher to consume.
/// </summary>
public sealed class ManifestService
{
    private readonly IManifestRepository _manifests;
    private readonly IObjectStorage _storage;

    private static readonly TimeSpan ChunkUrlExpiry = TimeSpan.FromMinutes(5);

    /// <summary>Initialises a new instance.</summary>
    public ManifestService(IManifestRepository manifests, IObjectStorage storage)
    {
        _manifests = manifests;
        _storage = storage;
    }

    /// <summary>
    /// Get the signed manifest for a specific version/platform/arch.
    /// Each chunk's CDN URL is a short-lived signed download URL.
    /// </summary>
    public async Task<ManifestDto?> GetForVersionAsync(
        Guid productId,
        Guid versionId,
        SupportedPlatform platform,
        CpuArchitecture arch,
        CancellationToken ct = default)
    {
        var manifest = await _manifests.GetForVersionAsync(productId, versionId, platform, arch, ct);
        if (manifest is null) return null;

        var chunks = new List<ChunkDto>(manifest.Chunks.Count);

        foreach (var chunk in manifest.Chunks.OrderBy(c => c.SequenceIndex))
        {
            var signedUrl = await _storage.GenerateSignedDownloadUrlAsync(
                "binaries", chunk.CdnPath, ChunkUrlExpiry, ct);

            chunks.Add(new ChunkDto(
                chunk.Id, chunk.SequenceIndex,
                chunk.OffsetInBinary, chunk.SizeBytes,
                chunk.HashSha256, chunk.Priority,
                signedUrl, chunk.Encrypted));
        }

        return new ManifestDto(
            manifest.Id,
            manifest.PlatformBinaryId,
            manifest.TotalSizeBytes,
            manifest.Signature,
            manifest.HashAlgorithm,
            manifest.ManifestHash,
            manifest.EncryptionAlgorithm,
            chunks);
    }
}
