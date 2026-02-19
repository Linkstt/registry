// <copyright file="ProductRepository.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Registry.Domain.Entities;
using Registry.Domain.Enums;
using Registry.Domain.Interfaces;
using Registry.Infrastructure.Data;

namespace Registry.Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="IProductRepository"/>.
/// </summary>
public sealed class ProductRepository : IProductRepository
{
    private readonly RegistryDbContext _db;

    /// <summary>Initialises a new instance.</summary>
    public ProductRepository(RegistryDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> ListAsync(
        Guid? developerId = null,
        ProductStatus? status = null,
        Guid? categoryId = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (developerId.HasValue)
            query = query.Where(p => p.DeveloperId == developerId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%") ||
                EF.Functions.ILike(p.ShortDescription, $"%{term}%"));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken ct = default)
    {
        var query = _db.Products.Where(p => p.Slug == slug);
        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return await query.AnyAsync(ct);
    }

    /// <inheritdoc/>
    public void Add(Product product) => _db.Products.Add(product);

    /// <inheritdoc/>
    public void Update(Product product) => _db.Products.Update(product);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
