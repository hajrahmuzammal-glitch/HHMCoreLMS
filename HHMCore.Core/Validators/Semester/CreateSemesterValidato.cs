using FluentValidation;
using HHMCore.Core.DTOs.Semester;

namespace HHMCore.Core.Validators.Semester;

public class CreateSemesterValidator : AbstractValidator<CreateSemesterDto>
{
    public CreateSemesterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Semester name is required.")
            .MinimumLength(3).WithMessage("Semester name must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Semester name cannot exceed 100 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .Must(d => d != default).WithMessage("A valid start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .Must(d => d != default).WithMessage("A valid end date is required.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.SemesterNumber)
    .InclusiveBetween(1, 8).WithMessage("Semester number must be between 1 and 8.");
    }
}
