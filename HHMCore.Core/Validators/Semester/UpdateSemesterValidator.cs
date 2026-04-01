using FluentValidation;
using HHMCore.Core.DTOs.Semester;

namespace HHMCore.Core.Validators.Semester;

public class UpdateSemesterValidator : AbstractValidator<UpdateSemesterDto>
{
    public UpdateSemesterValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Semester Id is required.")
            .Must(id => id != Guid.Empty).WithMessage("A valid Semester Id is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Semester name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.StartDate)
            .Must(d => d != default).WithMessage("A valid start date is required.")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .Must(d => d != default).WithMessage("A valid end date is required.")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate!.Value)
            .WithMessage("End date must be after start date.")
            .When(x => x.EndDate.HasValue && x.StartDate.HasValue);
    }
}