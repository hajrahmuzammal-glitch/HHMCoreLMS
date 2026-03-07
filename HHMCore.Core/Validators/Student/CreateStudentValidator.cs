using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HHMCore.Core.DTOs.Student;

namespace HHMCore.Core.Validators.Student
{
    public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
    {
        public CreateStudentValidator()
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
                .Matches(@"(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[\W])")
                .WithMessage("Password must contain uppercase, lowercase, number and special character.");

            RuleFor(x => x.RollNumber)
                .NotEmpty().WithMessage("Roll number is required.")
                .MaximumLength(20).WithMessage("Roll number cannot exceed 20 characters.");

            RuleFor(x => x.DepartmentId)
                .Must(id => id != Guid.Empty).WithMessage("A valid Department must be selected.");

            RuleFor(x => x.CurrentSemesterNumber)
                .GreaterThan(0).WithMessage("Semester number must be greater than 0.")
                .LessThanOrEqualTo(8).WithMessage("Semester number cannot exceed 8.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15).WithMessage("Phone number cannot exceed 15 characters.")
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.");
        }
    }
}