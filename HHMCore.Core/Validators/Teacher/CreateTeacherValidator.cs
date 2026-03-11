using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HHMCore.Core.DTOs.Teacher;

namespace HHMCore.Core.Validators.Teacher
{
    public class CreateTeacherValidator : AbstractValidator<CreateTeacherDto>
    {
        public CreateTeacherValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.Cnic)
                .NotEmpty()
                .Matches(@"^\d{13}$")
                .WithMessage("CNIC must be exactly 13 digits with no dashes.");
            
            RuleFor(x => x.Designation)
                .NotEmpty().WithMessage("Designation is required.")
                .MaximumLength(100).WithMessage("Designation cannot exceed 100 characters.");

            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero.");

            RuleFor(x => x.DepartmentId)
                .Must(id => id != Guid.Empty).WithMessage("A valid Department ID is required.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");

            RuleFor(x => x.Address)
                .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.");
        }
    }
}
