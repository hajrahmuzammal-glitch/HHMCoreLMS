using FluentValidation;
using HHMCore.Core.DTOs.Room;

namespace HHMCore.Core.Validators.Room;

public class UpdateRoomValidator : AbstractValidator<UpdateRoomDto>
{
    public UpdateRoomValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty).WithMessage("A valid room ID is required.");

        RuleFor(x => x.RoomNumber)
            .MaximumLength(20).WithMessage("Room number cannot exceed 20 characters.")
            .When(x => x.RoomNumber != null);

        RuleFor(x => x.Building)
            .MaximumLength(100).WithMessage("Building name cannot exceed 100 characters.")
            .When(x => x.Building != null);

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.")
            .When(x => x.Capacity.HasValue);

        RuleFor(x => x.RoomType)
            .IsInEnum().WithMessage("Invalid room type.")
            .When(x => x.RoomType.HasValue);
    }
}