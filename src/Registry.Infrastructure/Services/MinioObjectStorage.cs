// <copyright file="MinioObjectStorage.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Registry.Application.Interfaces;

namespace Registry.Infrastructure.Services;

/// <summary>
/// Production S3-compatible object storage backed by MinIO.
/// Used for presigned upload/download URLs and public CDN URL generation.
/// </summary>
public sealed class MinioObjectStorage : IObjectStorage
{
    private readonly IMinioClient _minio;
    private readonly string _cdnBaseUrl;
    private readonly ILogger<MinioObjectStorage> _logger;

    /// <summary>Initialises a new instance.</summary>
    public MinioObjectStorage(IConfiguration configuration, ILogger<MinioObjectStorage> logger)
    {
        _logger = logger;

        var endpoint = configuration["Storage:Endpoint"]
            ?? throw new InvalidOperationException("Storage:Endpoint is not configured.");
        var accessKey = configuration["Storage:AccessKey"]
            ?? throw new InvalidOperationException("Storage:AccessKey is not configured.");
        var secretKey = configuration["Storage:SecretKey"]
            ?? throw new InvalidOperationException("Storage:SecretKey is not configured.");
        var useSsl = configuration.GetValue("Storage:UseSsl", true);

        _cdnBaseUrl = (configuration["Storage:CdnBaseUrl"]
            ?? "https://cdn.allservices.cc").TrimEnd('/');

        var builder = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey);

        if (useSsl) builder = builder.WithSSL();

        _minio = builder.Build();

        _logger.LogInformation(
            "MinIO client configured — endpoint: {Endpoint}, SSL: {UseSsl}",
            endpoint, useSsl);
    }

    /// <inheritdoc/>
    public async Task<string> GeneratePresignedUploadUrlAsync(
        string bucket, string objectKey, string contentType,
        TimeSpan expiresIn, CancellationToken ct = default)
    {
        var args = new PresignedPutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry((int)expiresIn.TotalSeconds);

        var url = await _minio.PresignedPutObjectAsync(args);

        _logger.LogDebug(
            "Generated presigned upload URL for {Bucket}/{ObjectKey}, expires in {Seconds}s",
            bucket, objectKey, expiresIn.TotalSeconds);

        return url;
    }

    /// <inheritdoc/>
    public string GetPublicUrl(string bucket, string objectKey)
    {
        return $"{_cdnBaseUrl}/{bucket}/{objectKey}";
    }

    /// <inheritdoc/>
    public async Task<string> GenerateSignedDownloadUrlAsync(
        string bucket, string objectKey, TimeSpan expiresIn, CancellationToken ct = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry((int)expiresIn.TotalSeconds);

        var url = await _minio.PresignedGetObjectAsync(args);

        _logger.LogDebug(
            "Generated presigned download URL for {Bucket}/{ObjectKey}, expires in {Seconds}s",
            bucket, objectKey, expiresIn.TotalSeconds);

        return url;
    }
}
