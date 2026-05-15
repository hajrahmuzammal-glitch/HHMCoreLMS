using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.TimeSlot;
using HHMCore.Core.Enums;
using HHMCore.Core.Validators;
//using HHMCore.Core.Validators.TimeSlot;

namespace HHMCore.Tests.Validators.TimeSlot;

public class CreateTimeSlotValidatorTests
{
    private readonly CreateTimeSlotValidator _validator = new();

    private static CreateTimeSlotDto ValidDto() => new()
    {
        Days = LmsDaysOfWeek.Monday | LmsDaysOfWeek.Wednesday,
        StartTime = new TimeOnly(9, 0),
        EndTime = new TimeOnly(11, 0)
    };

    // ── Days ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Days_Zero_NoDaySelected_FailsValidation()
    {
        // LmsDaysOfWeek is a flags enum — value 0 means no day was selected
        // A time slot with no days is meaningless in a timetable
        var dto = ValidDto();
        dto.Days = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Days);
    }

    [Fact]
    public void Days_ValidSingleDay_PassesValidation()
    {
        var dto = ValidDto();
        dto.Days = LmsDaysOfWeek.Monday;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Days);
    }

    [Fact]
    public void Days_ValidMultipleDays_PassesValidation()
    {
        var dto = ValidDto();
        dto.Days = LmsDaysOfWeek.Monday | LmsDaysOfWeek.Wednesday | LmsDaysOfWeek.Friday;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Days);
    }

    // ── StartTime / EndTime ───────────────────────────────────────────────────

    [Fact]
    public void StartTime_IsDefault_FailsValidation()
    {
        // TimeOnly default is 00:00 — no real class starts at midnight
        // The validator must require a meaningful start time
        var dto = ValidDto();
        dto.StartTime = default;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void EndTime_IsDefault_FailsValidation()
    {
        var dto = ValidDto();
        dto.EndTime = default;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void ValidDto_AllFieldsCorrect_PassesValidation()
    {
        var result = _validator.TestValidate(ValidDto());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
