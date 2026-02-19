// <copyright file="CpuArchitecture.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

namespace Registry.Domain.Enums;

/// <summary>
/// CPU architecture a binary targets.
/// </summary>
public enum CpuArchitecture
{
    /// <summary>64-bit x86 (AMD64 / Intel 64).</summary>
    X64,

    /// <summary>64-bit ARM (Apple Silicon, Snapdragon, etc.).</summary>
    Arm64,

    /// <summary>Universal binary (fat binary on macOS).</summary>
    Universal,
}
