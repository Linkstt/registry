// <copyright file="CurrentUserService.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Registry.Application.Interfaces;

namespace Registry.Infrastructure.Services;

/// <summary>
/// Reads the authenticated user from the HTTP context.
/// </summary>
public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initialises a new instance.</summary>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc/>
    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User?.FindFirstValue("sub");

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    /// <inheritdoc/>
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    /// <inheritdoc/>
    public bool HasPermission(string permission)
    {
        var user = User;
        if (user is null) return false;

        // Identity packs all permissions into a single comma-separated "permissions" claim.
        // Also check individual "permission" claims for forward-compatibility.
        var hasIndividual = user.Claims
            .Where(c => c.Type is "permission" or "permissions")
            .Any(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(permission, StringComparer.OrdinalIgnoreCase));

        if (hasIndividual) return true;

        // For client_credentials tokens (service-to-service), fall back to scope.
        // e.g. scope "registry" grants registry.* permissions.
        var prefix = permission.Split('.')[0];
        return user.Claims
            .Any(c => c.Type == "scope" && c.Value.Equals(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public bool HasScope(string scope)
    {
        var user = User;
        if (user is null) return false;

        return user.Claims
            .Where(c => c.Type == "scope")
            .Any(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains(scope, StringComparer.OrdinalIgnoreCase));
    }
}
