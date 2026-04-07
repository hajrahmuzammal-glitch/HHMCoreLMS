using FluentValidation;
using HHMCore.Core.DTOs.Building;

namespace HHMCore.Core.Validators.Building
{
    public class UpdateBuildingValidator : AbstractValidator<UpdateBuildingDto>
    {
        public UpdateBuildingValidator()
        {
            RuleFor(x => x.Name)
                .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
                .When(x => x.Name != null);

            RuleFor(x => x.Code)
                .MaximumLength(10).WithMessage("Code cannot exceed 10 characters.")
                .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("Code can only contain letters, numbers, and hyphens.")
                .When(x => x.Code != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);
        }
    }
}