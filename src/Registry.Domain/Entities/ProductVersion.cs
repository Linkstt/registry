// <copyright file="ProductVersion.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Common;
using Registry.Domain.Enums;

namespace Registry.Domain.Entities;

/// <summary>
/// A specific version of a product, containing per-platform binaries.
/// </summary>
public class ProductVersion : Entity
{
    /// <summary>Owning product.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Semantic version string (e.g. "1.2.3").</summary>
    public string VersionString { get; set; } = string.Empty;

    /// <summary>Release channel.</summary>
    public ReleaseChannel Channel { get; set; } = ReleaseChannel.Stable;

    /// <summary>Changelog text (Markdown).</summary>
    public string? Changelog { get; set; }

    /// <summary>Release notes (Markdown).</summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>How this version was submitted.</summary>
    public VersionSource Source { get; set; }

    /// <summary>CI job ID if submitted via pipeline.</summary>
    public string? CiJobId { get; set; }

    /// <summary>Review pipeline status.</summary>
    public VersionStatus Status { get; set; } = VersionStatus.Uploading;

    /// <summary>If true, users must update to this version before launching.</summary>
    public bool IsForcedUpdate { get; set; }

    /// <summary>Staged rollout percentage (0–100). 100 = fully available.</summary>
    public int RolloutPercentage { get; set; } = 100;

    /// <summary>Minimum launcher version required to run this version.</summary>
    public string? MinimumLauncherVersion { get; set; }

    /// <summary>UTC timestamp when upload was initiated.</summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>UTC timestamp when version was approved.</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>UTC timestamp when version was yanked (if ever).</summary>
    public DateTime? YankedAt { get; set; }

    // ──── Navigation ────

    /// <summary>Parent product.</summary>
    public Product Product { get; set; } = null!;

    /// <summary>Per-platform binary builds.</summary>
    public ICollection<PlatformBinary> PlatformBinaries { get; set; } = [];
}
