// <copyright file="VersionSource.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// How a product version was submitted.
/// </summary>
public enum VersionSource
{
    /// <summary>Built and submitted via CI pipeline (Forgejo Actions).</summary>
    CiPipeline,

    /// <summary>Manually uploaded by the developer via the console.</summary>
    ManualUpload,
}
