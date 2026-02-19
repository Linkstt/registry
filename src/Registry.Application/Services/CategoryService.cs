// <copyright file="CategoryService.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Application.DTOs;
using Registry.Domain.Entities;
using Registry.Domain.Interfaces;

namespace Registry.Application.Services;

/// <summary>
/// Read-only category tree for marketplace filtering.
/// </summary>
public sealed class CategoryService
{
    private readonly ICategoryRepository _categories;

    /// <summary>Initialises a new instance.</summary>
    public CategoryService(ICategoryRepository categories) => _categories = categories;

    /// <summary>Get the full category tree.</summary>
    public async Task<IReadOnlyList<CategoryDto>> GetTreeAsync(CancellationToken ct = default)
    {
        var roots = await _categories.GetTreeAsync(ct);
        return roots.Select(MapToDto).ToList();
    }

    /// <summary>Get a single category by slug.</summary>
    public async Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var category = await _categories.GetBySlugAsync(slug, ct);
        return category is null ? null : MapToDto(category);
    }

    private static CategoryDto MapToDto(ProductCategory c) => new(
        c.Id, c.Name, c.Slug, c.Icon, c.Description, c.SortOrder,
        c.Children.OrderBy(ch => ch.SortOrder).Select(MapToDto).ToList());
}
