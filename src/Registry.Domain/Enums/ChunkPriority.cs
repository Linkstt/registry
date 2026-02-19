// <copyright file="ChunkPriority.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// Loading priority for a binary chunk.
/// </summary>
public enum ChunkPriority
{
    /// <summary>Must be loaded first — entry point / headers.</summary>
    Critical,

    /// <summary>Should be loaded early — core dependencies.</summary>
    High,

    /// <summary>Standard priority.</summary>
    Normal,

    /// <summary>Can be loaded on demand.</summary>
    Lazy,
}
