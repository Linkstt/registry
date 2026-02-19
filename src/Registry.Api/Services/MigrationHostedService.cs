// <copyright file="MigrationHostedService.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Registry.Infrastructure.Data;

namespace Registry.Api.Services;

/// <summary>
/// Runs EF Core migrations in the background after the host has started.
/// This allows the app to bind to its port and pass liveness health checks
/// even while waiting for the database to become available.
/// </summary>
public sealed class MigrationHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MigrationHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationHostedService"/>.
    /// </summary>
    public MigrationHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<MigrationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Applies pending EF Core migrations in the background after the host has started.
    /// Returns immediately so Kestrel can bind and pass liveness health checks.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Fire and forget — do NOT await here so Kestrel starts immediately
        _ = Task.Run(() => RunMigrationsAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task RunMigrationsAsync(CancellationToken cancellationToken)
    {
        const int maxRetries = 10;
        const int delaySeconds = 5;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<RegistryDbContext>();

                _logger.LogInformation("Applying database migrations (attempt {Attempt}/{Max})", attempt, maxRetries);
                await db.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migrations applied successfully");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex,
                    "Migration attempt {Attempt}/{Max} failed. Retrying in {Delay}s...",
                    attempt, maxRetries, delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "All {Max} migration attempts failed. Service will continue without migrations", maxRetries);
                // Don't throw — let the app stay up; readiness check will fail until DB is reachable
            }
        }
    }

    /// <summary>No-op stop.</summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
