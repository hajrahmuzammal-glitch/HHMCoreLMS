using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HHMCore.Core.DTOs.Teacher;

namespace HHMCore.Core.Validators.Teacher;

public sealed class UpdateTeacherProfileValidator : AbstractValidator<UpdateTeacherProfileDto>
{
    public UpdateTeacherProfileValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^03[0-9]{9}$").WithMessage("Phone number must be a valid Pakistani number (e.g. 03001234567).")
            .When(x => x.PhoneNumber != null);

        RuleFor(x => x.Qualification)
            .NotEmpty().WithMessage("Qualification is required.")
            .MaximumLength(150).WithMessage("Qualification cannot exceed 150 characters.")
            .When(x => x.Qualification != null);

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("Address cannot exceed 300 characters.")
            .When(x => x.Address != null);
    }
}