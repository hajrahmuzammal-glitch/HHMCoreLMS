using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HHMCore.Core.DTOs.Course;

namespace HHMCore.Core.Validators.Course;

public class UpdateCourseValidator : AbstractValidator<UpdateCourseDto>
{
    public UpdateCourseValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("A valid course ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
            .When(x => x.Name != null);

        RuleFor(x => x.Code)
            .MaximumLength(20).WithMessage("Code cannot exceed 20 characters.")
            .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("Code can only contain letters, numbers, and hyphens.")
            .When(x => x.Code != null);

        RuleFor(x => x.CreditHours)
            .InclusiveBetween(1, 6).WithMessage("Credit hours must be between 1 and 6.")
            .When(x => x.CreditHours.HasValue);

        RuleFor(x => x.SemesterNumber)
            .InclusiveBetween(1, 8).WithMessage("Semester number must be between 1 and 8.")
            .When(x => x.SemesterNumber.HasValue);

        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty).WithMessage("A valid department ID is required.")
            .When(x => x.DepartmentId.HasValue);
    }
}