using FluentValidation;
using HHMCore.Core.DTOs.Semester;

namespace HHMCore.Core.Validators.Semester;

public class UpdateSemesterValidator : AbstractValidator<UpdateSemesterDto>
{
    public UpdateSemesterValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Semester name cannot exceed 100 characters.")
            .MinimumLength(3).WithMessage("Semester name must be at least 3 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.StartDate)
            .Must(d => d!.Value != DateTime.MinValue).WithMessage("A valid start date is required.")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .Must(d => d!.Value != DateTime.MinValue).WithMessage("A valid end date is required.")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate!.Value)
            .WithMessage("End date must be after start date.")
            .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

        RuleFor(x => x.SemesterNumber)
            .InclusiveBetween(1, 8).WithMessage("Semester number must be between 1 and 8.")
            .When(x => x.SemesterNumber.HasValue);
    }
}
