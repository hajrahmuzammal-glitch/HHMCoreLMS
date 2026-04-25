namespace HHMCore.Core.Validators;

using FluentValidation;
using HHMCore.Core.DTOs.TimeSlot;

public class UpdateTimeSlotValidator : AbstractValidator<UpdateTimeSlotDto>
{
    public UpdateTimeSlotValidator()
    {
        RuleFor(x => x.Days)
            .Must(d => d != 0)
                .WithMessage("At least one day must be selected.")
            .Must(d => ((int)d!.Value & ~63) == 0)
                .WithMessage("Days contains an invalid value.")
            .When(x => x.Days.HasValue);

        RuleFor(x => x.StartTime)
            .Must(t => t!.Value != default(TimeOnly))
            .WithMessage("Start time cannot be midnight default.")
            .When(x => x.StartTime.HasValue);

        RuleFor(x => x.EndTime)
            .Must(t => t!.Value != default(TimeOnly))
            .WithMessage("End time cannot be midnight default.")
            .When(x => x.EndTime.HasValue);


        // Cross-field: only when both are supplied in the DTO
        When(x => x.StartTime.HasValue && x.EndTime.HasValue, () =>
        {
            RuleFor(x => x.EndTime!.Value)
                .Must((dto, endTime) => endTime > dto.StartTime!.Value)
                    .WithMessage("End time must be after start time.");
        });
    }
}
