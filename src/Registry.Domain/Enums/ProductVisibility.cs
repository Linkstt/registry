// <copyright file="ProductVisibility.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// Controls who can discover a product.
/// </summary>
public enum ProductVisibility
{
    /// <summary>Visible to everyone in the marketplace.</summary>
    Public,

    /// <summary>Accessible via direct link only.</summary>
    Unlisted,

    /// <summary>Only accessible to invited users.</summary>
    InviteOnly,
}
