// <copyright file="CategoriesController.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Registry.Application.DTOs;
using Registry.Application.Services;

namespace Registry.Api.Controllers;

/// <summary>
/// Product category browsing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    /// <summary>Initialises a new instance.</summary>
    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get the full category tree.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CategoryRead")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetTree(CancellationToken ct)
    {
        var tree = await _categoryService.GetTreeAsync(ct);
        return Ok(tree);
    }

    /// <summary>
    /// Get a category by slug.
    /// </summary>
    [HttpGet("{slug}")]
    [Authorize(Policy = "CategoryRead")]
    public async Task<ActionResult<CategoryDto>> GetBySlug(string slug, CancellationToken ct)
    {
        var category = await _categoryService.GetBySlugAsync(slug, ct);
        return category is null ? NotFound() : Ok(category);
    }
}
