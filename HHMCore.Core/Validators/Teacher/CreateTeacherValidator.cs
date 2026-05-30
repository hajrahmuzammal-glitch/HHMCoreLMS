using FluentValidation;
using HHMCore.Core.DTOs.Teacher;

namespace HHMCore.Core.Validators.Teacher;

public sealed class CreateTeacherValidator : AbstractValidator<CreateTeacherDto>
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

        RuleFor(x => x.DesignationId)
        .Must(id => id != Guid.Empty).WithMessage("A valid Designation ID is required.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Gender must be Male, Female, or Other.");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than zero.");

        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty).WithMessage("A valid Department ID is required.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^03[0-9]{9}$").WithMessage("Phone number must be a valid Pakistani number (e.g. 03001234567).");


        RuleFor(x => x.Address)
            .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.");
        // DateOfBirth rule
        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob >= new DateOnly(1940, 1, 1) &&
              dob <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18))
            .WithMessage("Date of birth must be after 1940 and candidate should not be younger than 18.");


        // JoiningDate rule  
        RuleFor(x => x.JoiningDate)
            .Must(jd => jd >= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30) &&
               jd <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1))
            .WithMessage("Joining date must be within the last 30 years and no more than 1 year in the future.")
;

        RuleFor(x => x.Qualification)
            .NotEmpty().WithMessage("Qualification is required is required.")
           .MaximumLength(150).WithMessage("Qualification cannot exceed 150 characters.");

    }
}
