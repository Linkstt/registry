// <copyright file="VersionDtos.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Enums;

namespace Registry.Application.DTOs;

/// <summary>Version summary returned in list endpoints.</summary>
public sealed record VersionSummaryDto(
    Guid Id,
    Guid ProductId,
    string VersionString,
    ReleaseChannel Channel,
    VersionSource Source,
    VersionStatus Status,
    int RolloutPercentage,
    DateTime UploadedAt,
    DateTime? ApprovedAt,
    DateTime? YankedAt);

/// <summary>Full version detail including platform binaries.</summary>
public sealed record VersionDetailDto(
    Guid Id,
    Guid ProductId,
    string VersionString,
    ReleaseChannel Channel,
    string? Changelog,
    string? ReleaseNotes,
    VersionSource Source,
    string? CiJobId,
    VersionStatus Status,
    bool IsForcedUpdate,
    int RolloutPercentage,
    string? MinimumLauncherVersion,
    DateTime UploadedAt,
    DateTime? ApprovedAt,
    DateTime? YankedAt,
    List<PlatformBinaryDto> PlatformBinaries);

/// <summary>A per-platform binary within a version.</summary>
public sealed record PlatformBinaryDto(
    Guid Id,
    SupportedPlatform Platform,
    CpuArchitecture Arch,
    long SizeBytes,
    Guid ManifestId);

/// <summary>Request to create a new version.</summary>
public sealed record CreateVersionRequest(
    string VersionString,
    ReleaseChannel Channel,
    string? Changelog,
    string? ReleaseNotes,
    VersionSource Source,
    string? CiJobId,
    bool IsForcedUpdate,
    int RolloutPercentage,
    string? MinimumLauncherVersion);

/// <summary>Request to yank a version.</summary>
public sealed record YankVersionRequest(
    string? Reason);
