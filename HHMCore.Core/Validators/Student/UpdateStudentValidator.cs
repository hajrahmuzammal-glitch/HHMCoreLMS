using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HHMCore.Core.DTOs.Student;

namespace HHMCore.Core.Validators.Student
{
    public class UpdateStudentValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentValidator()
        {
            RuleFor(x => x.Id)
                .Must(id => id != Guid.Empty).WithMessage("A valid Student ID is required.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.DepartmentId)
                .Must(id => id != Guid.Empty).WithMessage("A valid Department must be selected.");

            RuleFor(x => x.CurrentSemesterNumber)
                .GreaterThan(0).WithMessage("Semester number must be greater than 0.")
                .LessThanOrEqualTo(8).WithMessage("Semester number cannot exceed 8.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15).WithMessage("Phone number cannot exceed 15 characters.")
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
