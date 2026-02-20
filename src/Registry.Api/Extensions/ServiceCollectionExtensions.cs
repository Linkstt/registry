// <copyright file="ServiceCollectionExtensions.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Threading.RateLimiting;
using OpenIddict.Validation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Registry.Api.Extensions;

/// <summary>
/// Extension methods for configuring Registry API services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure OpenIddict token validation and claims-based authorization policies.
    /// </summary>
    public static IServiceCollection AddRegistryAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName)
    {
        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        services.AddOpenIddict()
            .AddValidation(options =>
            {
                var authority = configuration["Authentication:Authority"]
                    ?? throw new InvalidOperationException("Authentication:Authority is not configured.");

                options.SetIssuer(authority);
                options.AddAudiences(
                    configuration["Authentication:Audience"] ?? "registry_api");

                options.UseSystemNetHttp();
                options.UseAspNetCore();

                if (environmentName == "Development")
                {
                    options.Configure(o => o.TokenValidationParameters.ValidateIssuer = false);
                }
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("ProductRead", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        HasAnyCdnPermission(ctx.User, "admin.products.read", "developer.products.read") ||
                        HasScope(ctx.User, "developer") ||
                        HasScope(ctx.User, "admin") ||
                        HasScope(ctx.User, "store") ||
                        HasScope(ctx.User, "registry")))
            .AddPolicy("ProductWrite", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        HasAnyCdnPermission(ctx.User, "admin.products.write", "developer.products.create") ||
                        HasScope(ctx.User, "developer") ||
                        HasScope(ctx.User, "registry")))
            .AddPolicy("VersionRead", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        HasAnyCdnPermission(ctx.User, "admin.products.versions.read", "developer.products.read") ||
                        HasScope(ctx.User, "developer") ||
                        HasScope(ctx.User, "admin") ||
                        HasScope(ctx.User, "registry")))
            .AddPolicy("VersionWrite", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        HasAnyCdnPermission(ctx.User, "admin.products.versions.yank", "developer.products.create") ||
                        HasScope(ctx.User, "developer") ||
                        HasScope(ctx.User, "registry")))
            .AddPolicy("ManifestRead", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        HasPermission(ctx.User, "launcher.distribution.manifest") ||
                        HasScope(ctx.User, "license") ||
                        HasScope(ctx.User, "admin") ||
                        HasScope(ctx.User, "registry")))
            .AddPolicy("CategoryRead", policy =>
                policy.RequireAuthenticatedUser())
            .AddPolicy("AssetWrite", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        HasAnyCdnPermission(ctx.User, "developer.products.create", "admin.products.write") ||
                        HasScope(ctx.User, "developer") ||
                        HasScope(ctx.User, "registry")))
            .AddPolicy("AdminOnly", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireAssertion(ctx =>
                        HasPermission(ctx.User, "admin.products.write") ||
                        HasScope(ctx.User, "admin") ||
                        HasScope(ctx.User, "registry")));

        return services;
    }

    /// <summary>
    /// Register health checks for PostgreSQL connectivity.
    /// </summary>
    public static IServiceCollection AddRegistryHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// Configure OpenTelemetry metrics and tracing.
    /// </summary>
    public static IServiceCollection AddRegistryOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService("registry-service"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        return services;
    }

    /// <summary>
    /// Configure rate limiting.
    /// </summary>
    public static IServiceCollection AddRegistryRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Per-user rate limit for write operations
            options.AddPolicy("WriteOperation", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }));

            // Global rate limit: 300 requests per minute per IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }));
        });

        return services;
    }

    /// <summary>
    /// Configure CORS.
    /// </summary>
    public static IServiceCollection AddRegistryCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? [
                "https://platform.allservices.cc",
                "https://console.developer.allservices.cc",
                "https://admin.allservices.cc",
                "https://store.allservices.cc",
            ];

        services.AddCors(options =>
        {
            options.AddPolicy("RegistryCorsPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                    .WithExposedHeaders("X-Correlation-Id", "X-Total-Count")
                    .SetPreflightMaxAge(TimeSpan.FromHours(1));
            });
        });

        return services;
    }

    // ──── Permission helpers (handle comma-separated "permissions" claim from Identity) ────

    private static bool HasAnyCdnPermission(ClaimsPrincipal user, params string[] permissions)
    {
        return permissions.Any(p => HasPermission(user, p));
    }

    private static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        // Individual "permission" claims
        if (user.HasClaim("permission", permission))
        {
            return true;
        }

        // Comma-separated "permissions" claim from Identity
        var permissionsClaim = user.FindFirst("permissions")?.Value;
        if (permissionsClaim is not null)
        {
            return permissionsClaim
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(permission, StringComparer.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool HasScope(ClaimsPrincipal user, string scope)
    {
        if (user.HasClaim("scope", scope) || user.HasClaim("scp", scope))
        {
            return true;
        }

        var scopeClaim = user.FindFirst("scope")?.Value ?? user.FindFirst("scp")?.Value;
        if (scopeClaim is not null)
        {
            return scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(scope);
        }

        return false;
    }
}
