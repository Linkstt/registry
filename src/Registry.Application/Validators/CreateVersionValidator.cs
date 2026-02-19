// <copyright file="CreateVersionValidator.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using FluentValidation;
using Registry.Application.DTOs;

namespace Registry.Application.Validators;

/// <summary>
/// Validates <see cref="CreateVersionRequest"/>.
/// </summary>
public sealed class CreateVersionValidator : AbstractValidator<CreateVersionRequest>
{
    /// <summary>Initialises validation rules.</summary>
    public CreateVersionValidator()
    {
        RuleFor(x => x.VersionString)
            .NotEmpty().WithMessage("Version string is required.")
            .MaximumLength(40).WithMessage("Version string must not exceed 40 characters.")
            .Matches(@"^\d+\.\d+\.\d+(-[\w.]+)?(\+[\w.]+)?$")
            .WithMessage("Version string must follow semantic versioning (e.g. 1.2.3, 1.0.0-beta.1).");

        RuleFor(x => x.Channel)
            .IsInEnum().WithMessage("Invalid release channel.");

        RuleFor(x => x.Source)
            .IsInEnum().WithMessage("Invalid version source.");

        RuleFor(x => x.Changelog)
            .MaximumLength(50_000).WithMessage("Changelog must not exceed 50,000 characters.");

        RuleFor(x => x.ReleaseNotes)
            .MaximumLength(50_000).WithMessage("Release notes must not exceed 50,000 characters.");

        RuleFor(x => x.RolloutPercentage)
            .InclusiveBetween(0, 100).WithMessage("Rollout percentage must be between 0 and 100.");

        RuleFor(x => x.MinimumLauncherVersion)
            .MaximumLength(40).WithMessage("Minimum launcher version must not exceed 40 characters.");
    }
}
