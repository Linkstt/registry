// <copyright file="ReleaseChannel.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// Release channel for a product version.
/// </summary>
public enum ReleaseChannel
{
    /// <summary>Stable release.</summary>
    Stable,

    /// <summary>Beta pre-release.</summary>
    Beta,

    /// <summary>Nightly / bleeding-edge.</summary>
    Nightly,
}
