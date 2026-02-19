// <copyright file="VersionStatus.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// Lifecycle status of a product version.
/// </summary>
public enum VersionStatus
{
    /// <summary>Binary is being uploaded.</summary>
    Uploading,

    /// <summary>Processing (chunking, hashing, encrypting).</summary>
    Processing,

    /// <summary>Awaiting automated scan.</summary>
    ScanPending,

    /// <summary>Automated scan found issues.</summary>
    ScanFailed,

    /// <summary>Awaiting human review.</summary>
    ReviewPending,

    /// <summary>Approved — available for distribution.</summary>
    Approved,

    /// <summary>Rejected by reviewer.</summary>
    Rejected,

    /// <summary>Yanked — pulled from distribution after approval.</summary>
    Yanked,
}
