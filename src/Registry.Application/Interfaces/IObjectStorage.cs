// <copyright file="IObjectStorage.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Application.Interfaces;

/// <summary>
/// Abstraction over object storage (MinIO / R2 / S3-compatible).
/// Used for presigned uploads and CDN URL generation.
/// </summary>
public interface IObjectStorage
{
    /// <summary>
    /// Generate a presigned PUT URL for uploading an asset.
    /// </summary>
    /// <param name="bucket">Storage bucket.</param>
    /// <param name="objectKey">Object key / path.</param>
    /// <param name="contentType">Expected content type.</param>
    /// <param name="expiresIn">URL validity duration.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Presigned upload URL.</returns>
    Task<string> GeneratePresignedUploadUrlAsync(
        string bucket,
        string objectKey,
        string contentType,
        TimeSpan expiresIn,
        CancellationToken ct = default);

    /// <summary>
    /// Generate a public CDN URL for a stored object.
    /// </summary>
    /// <param name="bucket">Storage bucket.</param>
    /// <param name="objectKey">Object key / path.</param>
    /// <returns>Public URL.</returns>
    string GetPublicUrl(string bucket, string objectKey);

    /// <summary>
    /// Generate a short-lived signed URL for private chunk delivery.
    /// </summary>
    /// <param name="bucket">Storage bucket.</param>
    /// <param name="objectKey">Object key / path.</param>
    /// <param name="expiresIn">URL validity duration.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Signed download URL.</returns>
    Task<string> GenerateSignedDownloadUrlAsync(
        string bucket,
        string objectKey,
        TimeSpan expiresIn,
        CancellationToken ct = default);
}
