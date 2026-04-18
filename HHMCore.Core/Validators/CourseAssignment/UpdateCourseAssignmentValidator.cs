using FluentValidation;
using HHMCore.Core.DTOs.CourseAssignment;

namespace HHMCore.Core.Validators.CourseAssignment;

public class UpdateCourseAssignmentValidator : AbstractValidator<UpdateCourseAssignmentDto>
{
    public UpdateCourseAssignmentValidator()
    {
        RuleFor(x => x.TeacherId)
            .Must(id => id != Guid.Empty).WithMessage("A valid teacher is required.")
            .When(x => x.TeacherId.HasValue);

        RuleFor(x => x.SemesterId)
            .Must(id => id != Guid.Empty).WithMessage("A valid semester is required.")
            .When(x => x.SemesterId.HasValue);


        RuleFor(x => x.RoomId)
            .Must(id => id != Guid.Empty).WithMessage("A valid room is required.")
            .When(x => x.RoomId.HasValue);

        RuleFor(x => x.TimeSlotId)
            .Must(id => id != Guid.Empty).WithMessage("A valid time slot is required.")
            .When(x => x.TimeSlotId.HasValue);

        RuleFor(x => x.Section)
            .NotEmpty().WithMessage("Section cannot be empty.")
            .MaximumLength(10).WithMessage("Section cannot exceed 10 characters.")
            .When(x => x.Section != null);

        RuleFor(x => x.MaxEnrollment)
            .GreaterThan(0).WithMessage("Max enrollment must be greater than zero.")
            .When(x => x.MaxEnrollment.HasValue);
    }
}