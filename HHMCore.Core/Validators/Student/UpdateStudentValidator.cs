using FluentValidation;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Student;

namespace HHMCore.Core.Validators.Student;

public class UpdateStudentValidator : AbstractValidator<UpdateStudentDto>
{
    public UpdateStudentValidator()
    {
        RuleFor(x => x.FullName)
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.")
            .When(x => x.FullName != null);

        // Pakistani format: 03XXXXXXXXX
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^03[0-9]{9}$")
            .WithMessage("Phone number must be a valid Pakistani number (e.g. 03001234567).")
            .When(x => x.PhoneNumber != null);

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob!.Value < DateTime.UtcNow.AddYears(-15))
            .WithMessage("Student must be at least 15 years old.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob!.Value >= new DateTime(1950, 1, 1))
            .WithMessage("Date of birth cannot be before 1950.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.CurrentSemesterNumber)
            .InclusiveBetween(1, 8).WithMessage("Semester number must be between 1 and 8.")
            .When(x => x.CurrentSemesterNumber.HasValue);

        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty).WithMessage("A valid Department ID is required.")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.Status)
            .Must(s => s == StudentStatus.Active || s == StudentStatus.Inactive || s == StudentStatus.Suspended)
            .WithMessage("Status must be Active, Inactive, or Suspended.")
            .When(x => x.Status != null);
    }
}
