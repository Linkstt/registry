// <copyright file="ProductDto.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Registry.Domain.Enums;

namespace Registry.Application.DTOs;

/// <summary>Product summary returned in list endpoints.</summary>
public sealed record ProductSummaryDto(
    Guid Id,
    Guid DeveloperId,
    string Name,
    string Slug,
    string ShortDescription,
    ProductStatus Status,
    ProductVisibility Visibility,
    List<SupportedPlatform> PlatformSupport,
    TrustBadge TrustBadge,
    string? IconUrl,
    DateTime CreatedAt,
    DateTime? PublishedAt);

/// <summary>Full product detail.</summary>
public sealed record ProductDetailDto(
    Guid Id,
    Guid DeveloperId,
    string Name,
    string Slug,
    string ShortDescription,
    string LongDescription,
    Guid? CategoryId,
    string? CategoryName,
    List<string> Tags,
    ProductStatus Status,
    ProductVisibility Visibility,
    List<SupportedPlatform> PlatformSupport,
    TrustBadge TrustBadge,
    string? IconUrl,
    string? BannerUrl,
    List<string> ScreenshotUrls,
    string? TrailerUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);

/// <summary>Request to create a new product.</summary>
public sealed record CreateProductRequest(
    string Name,
    string Slug,
    string ShortDescription,
    string LongDescription,
    Guid? CategoryId,
    List<string>? Tags,
    ProductVisibility Visibility,
    List<SupportedPlatform> PlatformSupport,
    string? TrailerUrl);

/// <summary>Request to patch an existing product.</summary>
public sealed record UpdateProductRequest(
    string? Name,
    string? ShortDescription,
    string? LongDescription,
    Guid? CategoryId,
    List<string>? Tags,
    ProductVisibility? Visibility,
    List<SupportedPlatform>? PlatformSupport,
    string? TrailerUrl);
