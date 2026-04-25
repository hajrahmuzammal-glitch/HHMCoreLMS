using FluentAssertions;
using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.TimeSlot;
using HHMCore.Core.Enums;
using HHMCore.Core.Validators;
//using HHMCore.Core.Validators.TimeSlot;

namespace HHMCore.Tests.Validators.TimeSlot;

public class UpdateTimeSlotValidatorTests
{
    private readonly UpdateTimeSlotValidator _validator = new();

    // ── Days ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Days_ProvidedAsZero_FailsValidation()
    {
        // If caller explicitly sends Days = 0 they are clearing all days
        // That produces a time slot with no days — invalid even in an update
        var dto = new UpdateTimeSlotDto { Days = (LmsDaysOfWeek)0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Days);
    }

    [Fact]
    public void Days_ProvidedAndValid_PassesValidation()
    {
        var dto = new UpdateTimeSlotDto { Days = LmsDaysOfWeek.Thursday };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Days);
    }

    // ── StartTime ─────────────────────────────────────────────────────────────

    [Fact]
    public void StartTime_ProvidedAsDefault_FailsValidation()
    {
        // Explicitly sending TimeOnly default (00:00) means no real time was chosen
        var dto = new UpdateTimeSlotDto { StartTime = default(TimeOnly) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void StartTime_ProvidedAndValid_PassesValidation()
    {
        var dto = new UpdateTimeSlotDto { StartTime = new TimeOnly(10, 0) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
    }

    // ── EndTime ───────────────────────────────────────────────────────────────

    [Fact]
    public void EndTime_ProvidedAsDefault_FailsValidation()
    {
        var dto = new UpdateTimeSlotDto { EndTime = default(TimeOnly) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void EndTime_ProvidedAndValid_PassesValidation()
    {
        var dto = new UpdateTimeSlotDto { EndTime = new TimeOnly(12, 0) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    // ── Empty dto — most important test for UpdateValidator ───────────────────

    [Fact]
    public void EmptyDto_AllFieldsNull_PassesValidation()
    {
        // Sending nothing is valid — service retains all existing values
        // Validator must never block a null field in an update
        var dto = new UpdateTimeSlotDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
