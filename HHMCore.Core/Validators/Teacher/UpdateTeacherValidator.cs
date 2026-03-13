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
             .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.")
             .When(x => x.FullName is not null);

            RuleFor(x => x.DesignationId)
            .Must(id => id != Guid.Empty).WithMessage("A valid Designation ID is required.")
            .When(x => x.DesignationId.HasValue);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Gender must be Male, Female, or Other.")
                .When(x => x.Gender.HasValue);

            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero.")
                .When(x => x.Salary.HasValue);

            RuleFor(x => x.DepartmentId)
                .Must(id => id != Guid.Empty).WithMessage("A valid Department ID is required.")
                .When(x => x.DepartmentId.HasValue);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.Address)
                .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.")
                .When(x => x.Address != null);

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(dob => dob >= new DateTime(1940, 1, 1) && dob <= DateTime.UtcNow.AddYears(-22))
                .WithMessage("Date of birth must be between 1940 and 22 years ago.");
        }
    }
}
