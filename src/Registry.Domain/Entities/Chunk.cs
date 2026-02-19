// <copyright file="Chunk.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Common;
using Registry.Domain.Enums;

namespace Registry.Domain.Entities;

/// <summary>
/// A single encrypted chunk of a binary.
/// The launcher downloads and verifies these individually.
/// </summary>
public class Chunk : Entity
{
    /// <summary>Owning manifest.</summary>
    public Guid ManifestId { get; set; }

    /// <summary>Zero-based position within the manifest.</summary>
    public int SequenceIndex { get; set; }

    /// <summary>Byte offset in the reconstructed binary.</summary>
    public long OffsetInBinary { get; set; }

    /// <summary>Chunk size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>SHA-256 hash of the encrypted chunk content.</summary>
    public string HashSha256 { get; set; } = string.Empty;

    /// <summary>Loading priority — critical chunks are fetched first.</summary>
    public ChunkPriority Priority { get; set; } = ChunkPriority.Normal;

    /// <summary>Path in object storage / CDN.</summary>
    public string CdnPath { get; set; } = string.Empty;

    /// <summary>Whether this chunk is encrypted. Always true in production.</summary>
    public bool Encrypted { get; set; } = true;

    // ──── Navigation ────

    /// <summary>Parent manifest.</summary>
    public BinaryManifest Manifest { get; set; } = null!;
}
