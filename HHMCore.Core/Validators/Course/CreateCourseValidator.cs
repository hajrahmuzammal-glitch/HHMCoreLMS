using FluentValidation;
using HHMCore.Core.DTOs.Course;

namespace HHMCore.Core.Validators.Course;

public class CreateCourseValidator : AbstractValidator<CreateCourseDto>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Course name is required.")
            .MaximumLength(150).WithMessage("Course name cannot exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Course code is required.")
            .MaximumLength(20).WithMessage("Course code cannot exceed 20 characters.")
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("Code can only contain letters, numbers, and hyphens.");

        RuleFor(x => x.CreditHours)
            .GreaterThan(0).WithMessage("Credit hours must be greater than 0.")
            .LessThanOrEqualTo(6).WithMessage("Credit hours cannot exceed 6.");

        RuleFor(x => x.SemesterNumber)
            .GreaterThan(0).WithMessage("Semester number must be greater than 0.")
            .LessThanOrEqualTo(8).WithMessage("Semester number cannot exceed 8.");

        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty).WithMessage("A valid Department must be selected.");
    }
}
