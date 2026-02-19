// <copyright file="ProductCategory.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Common;

namespace Registry.Domain.Entities;

/// <summary>
/// A hierarchical category for organizing products.
/// </summary>
public class ProductCategory : Entity
{
    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-safe slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Optional parent category (for nested hierarchy).</summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>Icon identifier (class name or asset reference).</summary>
    public string? Icon { get; set; }

    /// <summary>Display order within the parent level.</summary>
    public int SortOrder { get; set; }

    /// <summary>Human-readable description.</summary>
    public string? Description { get; set; }

    // ──── Navigation ────

    /// <summary>Parent category.</summary>
    public ProductCategory? ParentCategory { get; set; }

    /// <summary>Child categories.</summary>
    public ICollection<ProductCategory> Children { get; set; } = [];

    /// <summary>Products in this category.</summary>
    public ICollection<Product> Products { get; set; } = [];
}
