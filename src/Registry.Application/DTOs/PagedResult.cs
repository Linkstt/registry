// <copyright file="PagedResult.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Application.DTOs;

/// <summary>
/// Generic paginated response wrapper.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>Total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>Whether there is a next page.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Whether there is a previous page.</summary>
    public bool HasPreviousPage => Page > 1;
}
