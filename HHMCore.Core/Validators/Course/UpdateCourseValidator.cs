using FluentValidation;
using HHMCore.Core.DTOs.Course;

namespace HHMCore.Core.Validators.Course;

public class UpdateCourseValidator : AbstractValidator<UpdateCourseDto>
{
    public UpdateCourseValidator()
    {

        RuleFor(x => x.Name)
                .MinimumLength(2).WithMessage("Course name must be at least 2 characters.")
                .MaximumLength(150).WithMessage("Course name cannot exceed 150 characters.")
                .When(x => x.Name != null);

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Course code cannot exceed 20 characters.")
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("Code can only contain letters, numbers, and hyphens.")
            .When(x => x.Code != null);

        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty).WithMessage("A valid Department ID is required.")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.CreditHours)
                    .InclusiveBetween(1, 6).WithMessage("Credit hours must be between 1 and 6.")
                    .When(x => x.CreditHours.HasValue);

        RuleFor(x => x.SemesterNumber)
            .InclusiveBetween(1, 8).WithMessage("Semester number must be between 1 and 8.")
            .When(x => x.SemesterNumber.HasValue);
    }
}