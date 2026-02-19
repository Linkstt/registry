// <copyright file="AssetDtos.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Enums;

namespace Registry.Application.DTOs;

/// <summary>Asset upload response with a presigned URL.</summary>
public sealed record AssetUploadDto(
    Guid AssetId,
    string UploadUrl,
    string CdnUrl);

/// <summary>Asset summary.</summary>
public sealed record AssetDto(
    Guid Id,
    Guid ProductId,
    AssetType Type,
    string CdnUrl,
    int? Width,
    int? Height,
    long SizeBytes,
    DateTime UploadedAt);
