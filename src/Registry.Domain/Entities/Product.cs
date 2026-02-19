// <copyright file="Product.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Common;
using Registry.Domain.Enums;

namespace Registry.Domain.Entities;

/// <summary>
/// A software product registered on the AllServices platform.
/// Owned by a developer and distributed via the marketplace.
/// </summary>
public class Product : Entity
{
    /// <summary>Identity user ID of the developer who owns this product.</summary>
    public Guid DeveloperId { get; set; }

    /// <summary>Product display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe unique slug (e.g. "my-cool-app").</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>One-liner shown in search results and cards.</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Full description in Markdown.</summary>
    public string LongDescription { get; set; } = string.Empty;

    /// <summary>Category this product belongs to.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Free-form tags for search and filtering.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Lifecycle status.</summary>
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    /// <summary>Who can discover this product.</summary>
    public ProductVisibility Visibility { get; set; } = ProductVisibility.Public;

    /// <summary>Platforms this product supports (any combination).</summary>
    public List<SupportedPlatform> PlatformSupport { get; set; } = [];

    /// <summary>Trust badge assigned by AllServices.</summary>
    public TrustBadge TrustBadge { get; set; } = TrustBadge.None;

    /// <summary>Asset ID of the product icon.</summary>
    public Guid? IconAssetId { get; set; }

    /// <summary>Asset ID of the product banner.</summary>
    public Guid? BannerAssetId { get; set; }

    /// <summary>Asset IDs of screenshots.</summary>
    public List<Guid> ScreenshotAssetIds { get; set; } = [];

    /// <summary>Optional trailer video URL (YouTube / direct).</summary>
    public string? TrailerUrl { get; set; }

    /// <summary>UTC timestamp when the product was first listed.</summary>
    public DateTime? PublishedAt { get; set; }

    // ──── Navigation ────

    /// <summary>Category navigation property.</summary>
    public ProductCategory? Category { get; set; }

    /// <summary>Versions of this product.</summary>
    public ICollection<ProductVersion> Versions { get; set; } = [];

    /// <summary>Media assets.</summary>
    public ICollection<Asset> Assets { get; set; } = [];
}
