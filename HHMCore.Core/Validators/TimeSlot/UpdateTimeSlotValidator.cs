using FluentValidation;
using HHMCore.Core.DTOs.TimeSlot;

namespace HHMCore.Core.Validators.TimeSlot;

public class UpdateTimeSlotValidator : AbstractValidator<UpdateTimeSlotDto>
{
    public UpdateTimeSlotValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty).WithMessage("A valid time slot ID is required.");

        RuleFor(x => x.Days)
            .Must(d => d != 0).WithMessage("At least one day must be selected.")
            .Must(d => ((int)d! & ~63) == 0).WithMessage("Invalid day selection.")
            .When(x => x.Days.HasValue);

        RuleFor(x => x)
            .Must(dto => dto.EndTime > dto.StartTime)
            .WithMessage("End time must be after start time.")
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue);
    }
}