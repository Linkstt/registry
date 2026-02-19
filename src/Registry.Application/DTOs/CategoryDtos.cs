// <copyright file="CategoryDtos.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Application.DTOs;

/// <summary>Category in a tree structure.</summary>
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Icon,
    string? Description,
    int SortOrder,
    List<CategoryDto> Children);
