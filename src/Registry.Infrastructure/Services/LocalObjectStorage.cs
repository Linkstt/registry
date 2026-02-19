// <copyright file="LocalObjectStorage.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Registry.Application.Interfaces;

namespace Registry.Infrastructure.Services;

/// <summary>
/// Local/development implementation of <see cref="IObjectStorage"/>.
/// In production this is replaced by an S3-compatible provider (MinIO / R2).
/// Generates URLs against a configurable CDN base URL.
/// </summary>
public sealed class LocalObjectStorage : IObjectStorage
{
    private readonly string _cdnBaseUrl;

    /// <summary>Initialises a new instance.</summary>
    public LocalObjectStorage(IConfiguration configuration)
    {
        _cdnBaseUrl = configuration["Storage:CdnBaseUrl"]
                      ?? "https://cdn.allservices.cc";
    }

    /// <inheritdoc/>
    public Task<string> GeneratePresignedUploadUrlAsync(
        string bucket, string objectKey, string contentType,
        TimeSpan expiresIn, CancellationToken ct = default)
    {
        // In production: S3 presigned PUT URL
        // Development: return a local API URL for file upload
        var url = $"{_cdnBaseUrl}/{bucket}/{objectKey}?upload=true&contentType={Uri.EscapeDataString(contentType)}&expires={expiresIn.TotalSeconds}";
        return Task.FromResult(url);
    }

    /// <inheritdoc/>
    public string GetPublicUrl(string bucket, string objectKey)
    {
        return $"{_cdnBaseUrl}/{bucket}/{objectKey}";
    }

    /// <inheritdoc/>
    public Task<string> GenerateSignedDownloadUrlAsync(
        string bucket, string objectKey, TimeSpan expiresIn, CancellationToken ct = default)
    {
        // In production: S3 presigned GET URL with expiry
        var url = $"{_cdnBaseUrl}/{bucket}/{objectKey}?token=dev&expires={expiresIn.TotalSeconds}";
        return Task.FromResult(url);
    }
}
