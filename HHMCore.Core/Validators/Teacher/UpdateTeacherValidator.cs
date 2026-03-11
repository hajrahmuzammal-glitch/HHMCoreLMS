using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HHMCore.Core.DTOs.Teacher;

namespace HHMCore.Core.Validators.Teacher
{
    public class UpdateTeacherValidator : AbstractValidator<UpdateTeacherDto>
    {
        public UpdateTeacherValidator()
        {
            RuleFor(x => x.Id)
                .Must(id => id != Guid.Empty).WithMessage("A valid Teacher ID is required.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.Designation)
                .NotEmpty().WithMessage("Designation is required.")
                .MaximumLength(100).WithMessage("Designation cannot exceed 100 characters.");

            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero.");

            RuleFor(x => x.DepartmentId)
                .Must(id => id != Guid.Empty).WithMessage("A valid Department ID is required.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.Address)
                .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.")
                .When(x => x.Address != null);

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
