// <copyright file="ICurrentUser.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Application.Interfaces;

/// <summary>
/// Provides the identity of the authenticated user from the HTTP context.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Subject identifier (Identity user ID).</summary>
    Guid? UserId { get; }

    /// <summary>Whether the user is authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Whether the user has a specific permission.</summary>
    bool HasPermission(string permission);

    /// <summary>Whether the user has a specific scope.</summary>
    bool HasScope(string scope);
}
