// <copyright file="ManifestDtos.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Enums;

namespace Registry.Application.DTOs;

/// <summary>Signed manifest returned to the launcher.</summary>
public sealed record ManifestDto(
    Guid Id,
    Guid PlatformBinaryId,
    long TotalSizeBytes,
    string Signature,
    string HashAlgorithm,
    string ManifestHash,
    string EncryptionAlgorithm,
    List<ChunkDto> Chunks);

/// <summary>A single chunk descriptor within a manifest.</summary>
public sealed record ChunkDto(
    Guid Id,
    int SequenceIndex,
    long OffsetInBinary,
    long SizeBytes,
    string HashSha256,
    ChunkPriority Priority,
    string CdnUrl,
    bool Encrypted);
