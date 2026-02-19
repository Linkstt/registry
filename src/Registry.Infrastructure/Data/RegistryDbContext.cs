// <copyright file="RegistryDbContext.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Registry.Domain.Entities;

namespace Registry.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the Software Registry.
/// </summary>
public sealed class RegistryDbContext : DbContext
{
    /// <summary>Initialises a new instance.</summary>
    public RegistryDbContext(DbContextOptions<RegistryDbContext> options) : base(options) { }

    /// <summary>Products.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Product versions.</summary>
    public DbSet<ProductVersion> ProductVersions => Set<ProductVersion>();

    /// <summary>Platform binaries.</summary>
    public DbSet<PlatformBinary> PlatformBinaries => Set<PlatformBinary>();

    /// <summary>Binary manifests.</summary>
    public DbSet<BinaryManifest> BinaryManifests => Set<BinaryManifest>();

    /// <summary>Binary chunks.</summary>
    public DbSet<Chunk> Chunks => Set<Chunk>();

    /// <summary>Product categories.</summary>
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    /// <summary>Product assets.</summary>
    public DbSet<Asset> Assets => Set<Asset>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("registry");

        // ──── Product ────
        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => e.Slug).IsUnique();
            b.HasIndex(e => e.DeveloperId);
            b.HasIndex(e => e.Status);

            b.Property(e => e.Name).HasMaxLength(120).IsRequired();
            b.Property(e => e.Slug).HasMaxLength(120).IsRequired();
            b.Property(e => e.ShortDescription).HasMaxLength(300).IsRequired();
            b.Property(e => e.LongDescription).IsRequired();
            b.Property(e => e.TrailerUrl).HasMaxLength(500);

            b.Property(e => e.Tags)
                .HasColumnType("text[]");

            b.Property(e => e.PlatformSupport)
                .HasColumnType("text[]")
                .HasConversion(
                    v => v.Select(p => p.ToString()).ToArray(),
                    v => v.Select(Enum.Parse<Domain.Enums.SupportedPlatform>).ToList());

            b.Property(e => e.ScreenshotAssetIds)
                .HasColumnType("uuid[]");

            b.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(e => e.Visibility)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(e => e.TrustBadge)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasMany(e => e.Versions)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(e => e.Assets)
                .WithOne(a => a.Product)
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVersion>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.ProductId, e.VersionString }).IsUnique();
            b.HasIndex(e => e.Status);

            b.Property(e => e.VersionString).HasMaxLength(40).IsRequired();
            b.Property(e => e.CiJobId).HasMaxLength(200);
            b.Property(e => e.MinimumLauncherVersion).HasMaxLength(40);

            b.Property(e => e.Channel)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(e => e.Source)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.HasMany(e => e.PlatformBinaries)
                .WithOne(pb => pb.Version)
                .HasForeignKey(pb => pb.VersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlatformBinary>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.VersionId, e.Platform, e.Arch }).IsUnique();

            b.Property(e => e.Platform)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.Property(e => e.Arch)
                .HasConversion<string>()
                .HasMaxLength(20);

            b.HasOne(e => e.Manifest)
                .WithOne(m => m.PlatformBinary)
                .HasForeignKey<BinaryManifest>(m => m.PlatformBinaryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ──── BinaryManifest ────
        modelBuilder.Entity<BinaryManifest>(b =>
        {
            b.HasKey(e => e.Id);

            b.Property(e => e.Signature).IsRequired();
            b.Property(e => e.HashAlgorithm).HasMaxLength(20).HasDefaultValue("SHA256");
            b.Property(e => e.ManifestHash).IsRequired();
            b.Property(e => e.EncryptionAlgorithm).HasMaxLength(20).HasDefaultValue("AES-256-GCM");
            b.Property(e => e.KeyRefId).HasMaxLength(200);

            b.HasMany(e => e.Chunks)
                .WithOne(c => c.Manifest)
                .HasForeignKey(c => c.ManifestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ──── Chunk ────
        modelBuilder.Entity<Chunk>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.ManifestId, e.SequenceIndex }).IsUnique();

            b.Property(e => e.HashSha256).HasMaxLength(64).IsRequired();
            b.Property(e => e.CdnPath).HasMaxLength(500).IsRequired();

            b.Property(e => e.Priority)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // ──── ProductCategory ────
        modelBuilder.Entity<ProductCategory>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => e.Slug).IsUnique();

            b.Property(e => e.Name).HasMaxLength(100).IsRequired();
            b.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            b.Property(e => e.Icon).HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);

            b.HasOne(e => e.ParentCategory)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ──── Asset ────
        modelBuilder.Entity<Asset>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => new { e.ProductId, e.Type });

            b.Property(e => e.CdnPath).HasMaxLength(500).IsRequired();

            b.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(20);
        });
    }
}
