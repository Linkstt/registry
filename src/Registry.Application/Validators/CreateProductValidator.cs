// <copyright file="CreateProductValidator.cs" company="AllServices">
// Copyright (c) AllServices. All rights reserved.
// </copyright>

using FluentValidation;
using Registry.Application.DTOs;

namespace Registry.Application.Validators;

/// <summary>
/// Validates <see cref="CreateProductRequest"/>.
/// </summary>
public sealed class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    /// <summary>Initialises validation rules.</summary>
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(120).WithMessage("Product name must not exceed 120 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(120).WithMessage("Slug must not exceed 120 characters.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens only.");

        RuleFor(x => x.ShortDescription)
            .NotEmpty().WithMessage("Short description is required.")
            .MaximumLength(300).WithMessage("Short description must not exceed 300 characters.");

        RuleFor(x => x.LongDescription)
            .NotEmpty().WithMessage("Long description is required.")
            .MaximumLength(50_000).WithMessage("Long description must not exceed 50,000 characters.");

        RuleFor(x => x.PlatformSupport)
            .NotEmpty().WithMessage("At least one platform must be specified.");

        RuleForEach(x => x.Tags)
            .MaximumLength(50).WithMessage("Each tag must not exceed 50 characters.");

        RuleFor(x => x.Tags)
            .Must(t => t is null || t.Count <= 20)
            .WithMessage("Maximum 20 tags allowed.");
    }
}
