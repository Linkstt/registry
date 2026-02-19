// <copyright file="Entity.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Common;

/// <summary>
/// Base entity with a primary key and audit timestamps.
/// </summary>
public abstract class Entity
{
    /// <summary>Gets the unique identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the UTC timestamp when the entity was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets the UTC timestamp when the entity was last updated.</summary>
    public DateTime UpdatedAt { get; set; }
}
