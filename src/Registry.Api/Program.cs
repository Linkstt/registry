// <copyright file="Program.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Registry.Api.Extensions;
using Registry.Api.Middleware;
using Registry.Api.Services;
using Registry.Application.Interfaces;
using Registry.Application.Services;
using Registry.Domain.Common;
using Registry.Domain.Interfaces;
using Registry.Infrastructure.Data;
using Registry.Infrastructure.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Registry Service");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId());

    // Database
    builder.Services.AddDbContext<RegistryDbContext>((sp, options) =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        options.AddInterceptors(new AuditSaveChangesInterceptor());
    });

    // Repositories
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IVersionRepository, VersionRepository>();
    builder.Services.AddScoped<IManifestRepository, ManifestRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IAssetRepository, AssetRepository>();

    // Application services
    builder.Services.AddScoped<ProductService>();
    builder.Services.AddScoped<VersionService>();
    builder.Services.AddScoped<ManifestService>();
    builder.Services.AddScoped<CategoryService>();
    builder.Services.AddScoped<AssetService>();

    // Infrastructure services
    builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
    builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    builder.Services.AddSingleton<IObjectStorage, MinioObjectStorage>();
    builder.Services.AddHttpContextAccessor();

    // Validation
    builder.Services.AddValidatorsFromAssemblyContaining<Registry.Application.Validators.CreateProductValidator>();

    // Auth + policies
    builder.Services.AddRegistryAuthentication(builder.Configuration, builder.Environment.EnvironmentName);

    // Health checks
    builder.Services.AddRegistryHealthChecks(builder.Configuration);

    // OpenTelemetry
    builder.Services.AddRegistryOpenTelemetry(builder.Configuration);

    // Rate limiting
    builder.Services.AddRegistryRateLimiting();

    // CORS
    builder.Services.AddRegistryCors(builder.Configuration);

    // Controllers
    builder.Services.AddControllers();

    // Background migration (runs after host starts, retries until DB is reachable)
    builder.Services.AddHostedService<MigrationHostedService>();

    var app = builder.Build();

    // Middleware pipeline
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseSerilogRequestLogging();
    app.UseRateLimiter();
    app.UseCors("RegistryCorsPolicy");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health/live", new()
    {
        Predicate = _ => false,
    });
    app.MapHealthChecks("/health/ready", new()
    {
        Predicate = check => check.Tags.Contains("ready"),
    });

    app.UseOpenTelemetryPrometheusScrapingEndpoint();

    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Registry Service terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
