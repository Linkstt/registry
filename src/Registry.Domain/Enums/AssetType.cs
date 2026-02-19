// <copyright file="AssetType.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// Type of product asset (media).
/// </summary>
public enum AssetType
{
    /// <summary>Product icon.</summary>
    Icon,

    /// <summary>Banner / header image.</summary>
    Banner,

    /// <summary>Screenshot.</summary>
    Screenshot,

    /// <summary>Trailer video thumbnail.</summary>
    TrailerThumbnail,
}
