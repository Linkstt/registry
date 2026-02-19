// <copyright file="ICategoryRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Entities;

namespace Registry.Domain.Interfaces;

/// <summary>
/// Repository for <see cref="ProductCategory"/> entities.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>Get a category by ID.</summary>
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Get a category by slug.</summary>
    Task<ProductCategory?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Get the full category tree (roots with nested children).</summary>
    Task<IReadOnlyList<ProductCategory>> GetTreeAsync(CancellationToken ct = default);

    /// <summary>Add a new category.</summary>
    void Add(ProductCategory category);

    /// <summary>Persist all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
