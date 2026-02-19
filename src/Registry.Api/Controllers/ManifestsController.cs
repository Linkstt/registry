// <copyright file="ManifestsController.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Registry.Application.DTOs;
using Registry.Application.Services;
using Registry.Domain.Enums;

namespace Registry.Api.Controllers;

/// <summary>
/// Binary manifest retrieval for the launcher's chunked-download engine.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ManifestsController : ControllerBase
{
    private readonly ManifestService _manifestService;
    private readonly ILogger<ManifestsController> _logger;

    /// <summary>Initialises a new instance.</summary>
    public ManifestsController(
        ManifestService manifestService,
        ILogger<ManifestsController> logger)
    {
        _manifestService = manifestService;
        _logger = logger;
    }

    /// <summary>
    /// Get the binary manifest for a specific product version, platform, and architecture.
    /// Returns chunk list with signed download URLs (5-minute TTL).
    /// </summary>
    [HttpGet("{productId:guid}/{versionId:guid}/{platform}/{arch}")]
    [Authorize(Policy = "ManifestRead")]
    public async Task<ActionResult<ManifestDto>> Get(
        Guid productId, Guid versionId, SupportedPlatform platform, CpuArchitecture arch,
        CancellationToken ct)
    {
        var manifest = await _manifestService.GetForVersionAsync(productId, versionId, platform, arch, ct);
        if (manifest is null) return NotFound();

        _logger.LogInformation(
            "Manifest served for product {ProductId} version {VersionId}, {Platform}/{Arch}",
            productId, versionId, platform, arch);

        return Ok(manifest);
    }
}
