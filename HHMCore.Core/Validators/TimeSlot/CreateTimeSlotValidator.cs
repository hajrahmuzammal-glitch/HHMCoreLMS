namespace HHMCore.Core.Validators;

using FluentValidation;
using HHMCore.Core.DTOs.TimeSlot;

public class CreateTimeSlotValidator : AbstractValidator<CreateTimeSlotDto>
{
    public CreateTimeSlotValidator()
    {
        RuleFor(x => x.Days)
            .Must(d => d != 0)
                .WithMessage("At least one day must be selected.")
            .Must(d => ((int)d & ~63) == 0)
                .WithMessage("Days contains an invalid value.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.EndTime)
            .Must((dto, endTime) => endTime > dto.StartTime)
                .WithMessage("End time must be after start time.");
    }
}