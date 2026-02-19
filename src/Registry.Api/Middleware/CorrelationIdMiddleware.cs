// <copyright file="CorrelationIdMiddleware.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Api.Middleware;

/// <summary>
/// Ensures every request has a correlation ID for distributed tracing.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string Header = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    /// <summary>Initialises a new instance.</summary>
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Invoke the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(Header))
        {
            context.Request.Headers[Header] = Guid.NewGuid().ToString("N");
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[Header] = context.Request.Headers[Header];
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
