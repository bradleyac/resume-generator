using System;
using FluentValidation;

namespace RGS.Backend.Shared.ViewModels;

public class ResumeDataValidator : AbstractValidator<ResumeDataModel>
{
  public ResumeDataValidator()
  {
    // RuleFor(rd => rd.About)
    //   .NotEmpty().WithMessage("About is required.");

    // RuleFor(rd => rd.City)
    //   .NotEmpty().WithMessage("City is required.");

    // RuleFor(rd => rd.Name).NotEmpty().WithMessage("Name is required.");

    // RuleFor(rd => rd.State).NotEmpty().WithMessage("State is required.");

    // RuleFor(rd => rd.StreetAddress).NotEmpty().WithMessage("Street Address is required.");

    // RuleFor(rd => rd.Title).NotEmpty().WithMessage("Title is required.");

    // RuleFor(rd => rd.Zip).NotEmpty().WithMessage("Zip is required.");

    RuleForEach(rd => rd.Skills).ChildRules((validator) =>
    {
      validator.RuleFor(cat => cat.Label).NotEmpty().WithMessage("Label is required.");
      validator.RuleForEach(cat => cat.Items).NotEmpty().WithMessage("Skill value is required.");
    });
  }
}
