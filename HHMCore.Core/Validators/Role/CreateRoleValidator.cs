using FluentValidation;
using HHMCore.Core.DTOs.Role;

namespace HHMCore.Core.Validators.Role
{
    public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MinimumLength(2).WithMessage("Role name must be at least 2 characters.")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.")
                .Matches(@"^[A-Za-z]+$").WithMessage("Role name can only contain letters. No spaces or special characters.");
        }
    }
}