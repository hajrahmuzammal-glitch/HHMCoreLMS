using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;
using HHMCore.Core.DTOs.Course;

namespace HHMCore.Core.Validators.Course;

public class CreateCourseValidator : AbstractValidator<CreateCourseDto>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Course name is required.")
            .MaximumLength(100).WithMessage("Course name cannot exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Course code is required.")
            .MaximumLength(20).WithMessage("Course code cannot exceed 20 characters.")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Course code can only contain letters and numbers.");

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