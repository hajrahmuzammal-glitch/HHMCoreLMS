using FluentValidation;
using HHMCore.Core.DTOs.CourseAssignment;

namespace HHMCore.Core.Validators.CourseAssignment;

public class UpdateCourseAssignmentValidator : AbstractValidator<UpdateCourseAssignmentDto>
{
    public UpdateCourseAssignmentValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty).WithMessage("A valid assignment ID is required.");

        RuleFor(x => x.TeacherId)
            .Must(id => id != Guid.Empty).WithMessage("A valid teacher is required.")
            .When(x => x.TeacherId.HasValue);

        RuleFor(x => x.CourseId)
            .Must(id => id != Guid.Empty).WithMessage("A valid course is required.")
            .When(x => x.CourseId.HasValue);

        RuleFor(x => x.SemesterId)
            .Must(id => id != Guid.Empty).WithMessage("A valid semester is required.")
            .When(x => x.SemesterId.HasValue);

        RuleFor(x => x.RoomId)
            .Must(id => id != Guid.Empty).WithMessage("A valid room is required.")
            .When(x => x.RoomId.HasValue);

        RuleFor(x => x.TimeSlotId)
            .Must(id => id != Guid.Empty).WithMessage("A valid time slot is required.")
            .When(x => x.TimeSlotId.HasValue);
    }
}