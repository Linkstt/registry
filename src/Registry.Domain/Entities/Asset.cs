// <copyright file="Asset.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Common;
using Registry.Domain.Enums;

namespace Registry.Domain.Entities;

/// <summary>
/// A media asset (icon, banner, screenshot, trailer thumbnail) attached to a product.
/// </summary>
public class Asset : Entity
{
    /// <summary>Owning product.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Kind of asset.</summary>
    public AssetType Type { get; set; }

    /// <summary>Path in object storage / CDN.</summary>
    public string CdnPath { get; set; } = string.Empty;

    /// <summary>Image width in pixels.</summary>
    public int? Width { get; set; }

    /// <summary>Image height in pixels.</summary>
    public int? Height { get; set; }

    /// <summary>File size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>UTC timestamp when asset was uploaded.</summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>Identity user ID of the uploader.</summary>
    public Guid UploadedBy { get; set; }

    // ──── Navigation ────

    /// <summary>Parent product.</summary>
    public Product Product { get; set; } = null!;
}
