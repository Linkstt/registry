// <copyright file="PlatformBinary.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Common;
using Registry.Domain.Enums;

namespace Registry.Domain.Entities;

/// <summary>
/// A compiled binary for a specific platform + architecture combination
/// within a product version.
/// </summary>
public class PlatformBinary : Entity
{
    /// <summary>Owning version.</summary>
    public Guid VersionId { get; set; }

    /// <summary>Target operating system.</summary>
    public SupportedPlatform Platform { get; set; }

    /// <summary>Target CPU architecture.</summary>
    public CpuArchitecture Arch { get; set; }

    /// <summary>Associated manifest describing binary chunks.</summary>
    public Guid ManifestId { get; set; }

    /// <summary>Total binary size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Chunk ID that contains the entry point.</summary>
    public Guid? EntryPointChunkId { get; set; }

    // ──── Navigation ────

    /// <summary>Parent version.</summary>
    public ProductVersion Version { get; set; } = null!;

    /// <summary>Binary manifest with chunk details.</summary>
    public BinaryManifest Manifest { get; set; } = null!;
}
