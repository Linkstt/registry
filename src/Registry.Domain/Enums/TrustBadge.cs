// <copyright file="TrustBadge.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// Trust badge assigned by AllServices to a product.
/// </summary>
public enum TrustBadge
{
    /// <summary>No special badge.</summary>
    None,

    /// <summary>Verified by AllServices staff.</summary>
    Verified,

    /// <summary>Featured product — editorially selected.</summary>
    Featured,
}
