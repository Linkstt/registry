// <copyright file="IProductRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Entities;
using Registry.Domain.Enums;

namespace Registry.Domain.Interfaces;

/// <summary>
/// Repository for <see cref="Product"/> aggregate roots.
/// </summary>
public interface IProductRepository
{
    /// <summary>Get a product by its unique identifier.</summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Get a product by its slug.</summary>
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// List products with filtering, sorting, and pagination.
    /// </summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> ListAsync(
        Guid? developerId = null,
        ProductStatus? status = null,
        Guid? categoryId = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>Check whether a slug is already taken.</summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken ct = default);

    /// <summary>Add a new product.</summary>
    void Add(Product product);

    /// <summary>Mark a product as modified.</summary>
    void Update(Product product);

    /// <summary>Persist all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
