
using FluentValidation;
using HHMCore.Core.DTOs.CourseAssignment;

namespace HHMCore.Core.Validators.CourseAssignment;

public class UpdateCourseAssignmentValidator
    : AbstractValidator<UpdateCourseAssignmentDto>
{
    public UpdateCourseAssignmentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("CourseAssignment Id is required.")
            .Must(id => id != Guid.Empty).WithMessage("A valid Id is required.");

        RuleFor(x => x.Room)
            .MaximumLength(50).WithMessage("Room cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Room));

        RuleFor(x => x.Schedule)
            .MaximumLength(200).WithMessage("Schedule cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Schedule));
    }
}