// <copyright file="ProductStatus.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// Lifecycle status of a product.
/// </summary>
public enum ProductStatus
{
    /// <summary>Product is a draft — not yet submitted for review.</summary>
    Draft,

    /// <summary>Product is under review.</summary>
    InReview,

    /// <summary>Product is publicly listed in the marketplace.</summary>
    Listed,

    /// <summary>Product has been suspended by an admin.</summary>
    Suspended,

    /// <summary>Product has been permanently delisted.</summary>
    Delisted,
}
