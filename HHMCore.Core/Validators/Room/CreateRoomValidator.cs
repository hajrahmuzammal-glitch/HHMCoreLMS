using FluentValidation;
using HHMCore.Core.DTOs.Room;

namespace HHMCore.Core.Validators.Room;

public class CreateRoomValidator : AbstractValidator<CreateRoomDto>
{
    public CreateRoomValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Room number is required.")
            .MaximumLength(20).WithMessage("Room number cannot exceed 20 characters.");

        RuleFor(x => x.Building)
            .NotEmpty().WithMessage("Building name is required.")
            .MaximumLength(100).WithMessage("Building name cannot exceed 100 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.");

        RuleFor(x => x.RoomType)
            .IsInEnum().WithMessage("Invalid room type.");
    }
}