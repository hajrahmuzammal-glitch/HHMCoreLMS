using FluentValidation;
using HHMCore.Core.DTOs.Teacher;

namespace HHMCore.Core.Validators.Teacher;

public sealed class UpdateTeacherValidator : AbstractValidator<UpdateTeacherDto>
{
    public UpdateTeacherValidator()
    {

        RuleFor(x => x.FullName)
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
            .Must(dob => dob!.Value >= new DateOnly(1940, 1, 1) &&
                         dob.Value <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18))
            .WithMessage("Date of birth must be between 1940 and 18 years ago.")
            .When(x => x.DateOfBirth.HasValue);


        RuleFor(x => x.JoiningDate)
            .Must(jd => jd!.Value >= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30) &&
                    jd.Value <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1))
            .WithMessage("Joining date must be within the last 30 years and no more than 1 year in the future.")
            .When(x => x.JoiningDate.HasValue);


        //added this rule too myself admin side
        RuleFor(x => x.Qualification)
           //.NotEmpty().WithMessage("Qualification is required.")
           .MaximumLength(150).WithMessage("Qualification cannot exceed 150 characters.")
           .When(x => x.Qualification != null);



    }
}
