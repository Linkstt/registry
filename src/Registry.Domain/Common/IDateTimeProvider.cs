// <copyright file="IDateTimeProvider.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Common;

/// <summary>
/// Abstraction over <see cref="DateTime"/> for testability.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>Gets the current UTC time.</summary>
    DateTime UtcNow { get; }
}
