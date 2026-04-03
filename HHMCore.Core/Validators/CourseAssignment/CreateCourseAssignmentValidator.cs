
using FluentValidation;
using HHMCore.Core.DTOs.CourseAssignment;

namespace HHMCore.Core.Validators.CourseAssignment;

public class CreateCourseAssignmentValidator
    : AbstractValidator<CreateCourseAssignmentDto>
{
    public CreateCourseAssignmentValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Teacher is required.")
            .Must(id => id != Guid.Empty).WithMessage("A valid Teacher Id is required.");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Course is required.")
            .Must(id => id != Guid.Empty).WithMessage("A valid Course Id is required.");

        RuleFor(x => x.SemesterId)
            .NotEmpty().WithMessage("Semester is required.")
            .Must(id => id != Guid.Empty).WithMessage("A valid Semester Id is required.");

        RuleFor(x => x.RoomId)
            .Must(id => id != Guid.Empty).WithMessage("A valid room is required.");

        RuleFor(x => x.TimeSlotId)
            .Must(id => id != Guid.Empty).WithMessage("A valid time slot is required.");
    }
}