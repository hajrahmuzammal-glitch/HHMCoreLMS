using FluentValidation;
using HHMCore.Core.DTOs.Room;

namespace HHMCore.Core.Validators.Room;

public class UpdateRoomValidator : AbstractValidator<UpdateRoomDto>
{
    public UpdateRoomValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Room number cannot be empty.")
            .MaximumLength(20).WithMessage("Room number cannot exceed 20 characters.")
            .When(x => x.RoomNumber != null);

        RuleFor(x => x.BuildingId)
             .Must(id => id != Guid.Empty).WithMessage("A valid Building ID is required.")
                .When(x => x.BuildingId.HasValue);

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.")
            .When(x => x.Capacity.HasValue);

        RuleFor(x => x.RoomType)
            .IsInEnum().WithMessage("Invalid room type.")
            .When(x => x.RoomType.HasValue);
    }
}
