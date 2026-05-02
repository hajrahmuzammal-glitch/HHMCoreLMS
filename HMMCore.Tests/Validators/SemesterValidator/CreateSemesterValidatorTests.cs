using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Semester;
using HHMCore.Core.Validators.Semester;

namespace HHMCore.Tests.Validators;

public sealed class CreateSemesterValidatorTests
{
    private readonly CreateSemesterValidator _sut = new();

    private static CreateSemesterDto Valid() => new()
    {
        Name = "Fall 2025",
        SemesterNumber = 1,
        StartDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)
    };

    // ── Name ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Name_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.Name = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_Exceeds100Chars_ShouldHaveError()
    {
        var dto = Valid(); dto.Name = new string('A', 101);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_Exactly100Chars_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Name = new string('A', 100);
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_TwoChars_ShouldHaveError()
    {
        var dto = Valid(); dto.Name = "AB";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ThreeChars_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Name = "ABC";
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }
    [Fact]
    public void Name_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── StartDate ──────────────────────────────────────────────────────────────

    [Fact]
    public void StartDate_Default_ShouldHaveError()
    {
        var dto = Valid(); dto.StartDate = default;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void StartDate_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    // ── EndDate ────────────────────────────────────────────────────────────────

    [Fact]
    public void EndDate_Default_ShouldHaveError()
    {
        var dto = Valid(); dto.EndDate = default;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void EndDate_BeforeStartDate_ShouldHaveError()
    {
        var dto = Valid();
        dto.StartDate = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        dto.EndDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void EndDate_EqualToStartDate_ShouldHaveError()
    {
        var dto = Valid();
        dto.StartDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        dto.EndDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void EndDate_AfterStartDate_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    // ── SemesterNumber ─────────────────────────────────────────────────────────
    [Fact]
    public void SemesterNumber_Zero_ShouldHaveError()
    {
        var dto = Valid(); dto.SemesterNumber = 0;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SemesterNumber);
    }

    [Fact]
    public void SemesterNumber_Nine_ShouldHaveError()
    {
        var dto = Valid(); dto.SemesterNumber = 9;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SemesterNumber);
    }

    [Fact]
    public void SemesterNumber_Eight_ShouldNotHaveError()
    {
        var dto = Valid(); dto.SemesterNumber = 8;
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.SemesterNumber);
    }
    // ── Full happy path ────────────────────────────────────────────────────────

    [Fact]
    public void ValidDto_ShouldHaveNoErrors()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }
}
