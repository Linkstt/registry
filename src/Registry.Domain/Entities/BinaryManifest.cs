// <copyright file="BinaryManifest.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Common;

namespace Registry.Domain.Entities;

/// <summary>
/// Describes the chunk layout of a platform binary.
/// Signed by the AllServices internal CA — this is the trust anchor.
/// </summary>
public class BinaryManifest : Entity
{
    /// <summary>Owning platform binary.</summary>
    public Guid PlatformBinaryId { get; set; }

    /// <summary>Total size of the reconstructed binary in bytes.</summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>Signature produced by the signing service (Base64).</summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>Hash algorithm used for individual chunks.</summary>
    public string HashAlgorithm { get; set; } = "SHA256";

    /// <summary>SHA-256 hash of the manifest payload itself.</summary>
    public string ManifestHash { get; set; } = string.Empty;

    /// <summary>Encryption algorithm used for chunks.</summary>
    public string EncryptionAlgorithm { get; set; } = "AES-256-GCM";

    /// <summary>Reference to the master key in Key Management Service (never stored here).</summary>
    public string? KeyRefId { get; set; }

    // ──── Navigation ────

    /// <summary>Parent binary.</summary>
    public PlatformBinary PlatformBinary { get; set; } = null!;

    /// <summary>Ordered chunk list.</summary>
    public ICollection<Chunk> Chunks { get; set; } = [];
}
