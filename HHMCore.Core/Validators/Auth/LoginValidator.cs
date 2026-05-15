using FluentValidation;
using HHMCore.Core.DTOs.Auth;
namespace HHMCore.Core.Validators.Auth;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}