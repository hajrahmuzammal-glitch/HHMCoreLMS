using FluentAssertions;
using HHMCore.Core.DTOs.CourseAssignment;
using HHMCore.Core.Validators.CourseAssignment;

namespace HHMCore.Tests.Validators;

public class UpdateCourseAssignmentValidatorTests
{
    private readonly UpdateCourseAssignmentValidator _validator = new();

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public void EmptyDto_ShouldPass() // all fields optional
    {
        var result = _validator.Validate(new UpdateCourseAssignmentDto());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidPartialDto_ShouldPass()
    {
        var dto = new UpdateCourseAssignmentDto { TeacherId = Guid.NewGuid(), Section = "B" };
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    // ── Guid fields (only when provided) ─────────────────────────────────

    [Fact]
    public void EmptyTeacherId_WhenProvided_ShouldFail()
    {
        var dto = new UpdateCourseAssignmentDto { TeacherId = Guid.Empty };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyRoomId_WhenProvided_ShouldFail()
    {
        var dto = new UpdateCourseAssignmentDto { RoomId = Guid.Empty };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyTimeSlotId_WhenProvided_ShouldFail()
    {
        var dto = new UpdateCourseAssignmentDto { TimeSlotId = Guid.Empty };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    // ── Section ───────────────────────────────────────────────────────────

    [Fact]
    public void EmptySection_WhenProvided_ShouldFail()
    {
        var dto = new UpdateCourseAssignmentDto { Section = "" };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void SectionExceedsMaxLength_ShouldFail()
    {
        var dto = new UpdateCourseAssignmentDto { Section = "ABCDEFGHIJK" };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    // ── MaxEnrollment ─────────────────────────────────────────────────────

    [Fact]
    public void ZeroMaxEnrollment_WhenProvided_ShouldFail()
    {
        var dto = new UpdateCourseAssignmentDto { MaxEnrollment = 0 };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void NegativeMaxEnrollment_WhenProvided_ShouldFail()
    {
        var dto = new UpdateCourseAssignmentDto { MaxEnrollment = -1 };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}