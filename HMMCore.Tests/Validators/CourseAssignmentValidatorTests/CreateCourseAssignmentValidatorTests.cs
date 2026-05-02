using FluentAssertions;
using HHMCore.Core.DTOs.CourseAssignment;
using HHMCore.Core.Validators.CourseAssignment;

namespace HHMCore.Tests.Validators;

public class CreateCourseAssignmentValidatorTests
{
    private readonly CreateCourseAssignmentValidator _validator = new();

    private static CreateCourseAssignmentDto ValidDto() => new()
    {
        TeacherId = Guid.NewGuid(),
        CourseId = Guid.NewGuid(),
        SemesterId = Guid.NewGuid(),
        RoomId = Guid.NewGuid(),
        TimeSlotId = Guid.NewGuid(),
        Section = "A",
        MaxEnrollment = 30
    };

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public void ValidDto_ShouldPass()
    {
        var result = _validator.Validate(ValidDto());
        result.IsValid.Should().BeTrue();
    }

    // ── Guid fields ───────────────────────────────────────────────────────

    [Fact]
    public void EmptyTeacherId_ShouldFail()
    {
        var dto = ValidDto(); dto.TeacherId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyCourseId_ShouldFail()
    {
        var dto = ValidDto(); dto.CourseId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmptySemesterId_ShouldFail()
    {
        var dto = ValidDto(); dto.SemesterId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyRoomId_ShouldFail()
    {
        var dto = ValidDto(); dto.RoomId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyTimeSlotId_ShouldFail()
    {
        var dto = ValidDto(); dto.TimeSlotId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    // ── Section ───────────────────────────────────────────────────────────

    [Fact]
    public void EmptySection_ShouldFail()
    {
        var dto = ValidDto(); dto.Section = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void SectionExceedsMaxLength_ShouldFail()
    {
        var dto = ValidDto(); dto.Section = "ABCDEFGHIJK"; // 11 chars
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    // ── MaxEnrollment ─────────────────────────────────────────────────────

    [Fact]
    public void ZeroMaxEnrollment_ShouldFail()
    {
        var dto = ValidDto(); dto.MaxEnrollment = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void NegativeMaxEnrollment_ShouldFail()
    {
        var dto = ValidDto(); dto.MaxEnrollment = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}