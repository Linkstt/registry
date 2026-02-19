// <copyright file="CategoryRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Registry.Domain.Entities;
using Registry.Domain.Interfaces;
using Registry.Infrastructure.Data;

namespace Registry.Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="ICategoryRepository"/>.
/// </summary>
public sealed class CategoryRepository : ICategoryRepository
{
    private readonly RegistryDbContext _db;

    /// <summary>Initialises a new instance.</summary>
    public CategoryRepository(RegistryDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.ProductCategories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<ProductCategory?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.ProductCategories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductCategory>> GetTreeAsync(CancellationToken ct = default)
    {
        // Load all categories then build tree in-memory (category count is small)
        var all = await _db.ProductCategories
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        var lookup = all.ToLookup(c => c.ParentCategoryId);

        foreach (var category in all)
        {
            category.Children = lookup[category.Id].OrderBy(c => c.SortOrder).ToList();
        }

        return all.Where(c => c.ParentCategoryId is null).ToList();
    }

    /// <inheritdoc/>
    public void Add(ProductCategory category) => _db.ProductCategories.Add(category);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
