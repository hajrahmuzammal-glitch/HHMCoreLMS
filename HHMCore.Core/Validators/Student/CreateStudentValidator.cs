using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HHMCore.Core.DTOs.Student;

namespace HHMCore.Core.Validators.Student;

public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters.")
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
            .Must(id => id != Guid.Empty).WithMessage("A valid department must be selected.");

        RuleFor(x => x.CurrentSemesterNumber)
            .InclusiveBetween(1, 8).WithMessage("Semester number must be between 1 and 8.");

        // Pakistani mobile numbers — must start with 03 followed by 9 digits
        RuleFor(x => x.PhoneNumber)
           .NotEmpty().WithMessage("Phone Number is required.")
            .Matches(@"^03[0-9]{9}$")
            .WithMessage("Phone number must be a valid Pakistani number (e.g. 03001234567).")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required.")
            .Must(dob => dob < DateTime.UtcNow.AddYears(-15))
            .WithMessage("Student must be at least 15 years old.")
            .Must(dob => dob >= new DateTime(1950, 1, 1))
            .WithMessage("Date of birth cannot be before 1950.");

        RuleFor(x => x.Address)
.NotEmpty()
.WithMessage("Address is required.")
.Must(a => !string.IsNullOrWhiteSpace(a))
.WithMessage("Valid Address is required!.")
.MaximumLength(250)
.WithMessage("Address cannot exceed 350 characters.");
    }
}