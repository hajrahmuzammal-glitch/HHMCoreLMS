using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Semester;
using HHMCore.Core.Validators.Semester;

namespace HHMCore.Tests.Validators;

public sealed class UpdateSemesterValidatorTests
{
    private readonly UpdateSemesterValidator _sut = new();

    // ── Rule 5: empty DTO must pass ────────────────────────────────────────────

    [Fact]
    public void EmptyDto_AllNull_ShouldHaveNoErrors()
    {
        _sut.TestValidate(new UpdateSemesterDto()).ShouldNotHaveAnyValidationErrors();
    }

    // ── Name ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Name_Null_ShouldNotHaveError()
    {
        var dto = new UpdateSemesterDto { Name = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ProvidedAndExceeds100Chars_ShouldHaveError()
    {
        var dto = new UpdateSemesterDto { Name = new string('A', 101) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateSemesterDto { Name = "Spring 2026" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── StartDate ──────────────────────────────────────────────────────────────

    [Fact]
    public void StartDate_Null_ShouldNotHaveError()
    {
        var dto = new UpdateSemesterDto { StartDate = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void StartDate_ProvidedAsDefault_ShouldHaveError()
    {
        var dto = new UpdateSemesterDto { StartDate = DateTime.MinValue };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void StartDate_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateSemesterDto
        {
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    // ── EndDate ────────────────────────────────────────────────────────────────

    [Fact]
    public void EndDate_Null_ShouldNotHaveError()
    {
        var dto = new UpdateSemesterDto { EndDate = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void EndDate_ProvidedAsDefault_ShouldHaveError()
    {
        var dto = new UpdateSemesterDto { EndDate = DateTime.MinValue };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void EndDate_BothProvided_EndBeforeStart_ShouldHaveError()
    {
        var dto = new UpdateSemesterDto
        {
            StartDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void EndDate_OnlyEndDateProvided_ShouldNotHaveError()
    {
        // StartDate not provided — cross-field rule is skipped by .When()
        var dto = new UpdateSemesterDto
        {
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc)
        };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void EndDate_BothProvided_EndAfterStart_ShouldNotHaveError()
    {
        var dto = new UpdateSemesterDto
        {
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc)
        };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
    // ── SemesterNumber ─────────────────────────────────────────────────────────
    [Fact]
    public void SemesterNumber_Null_ShouldNotHaveError()
    {
        var dto = new UpdateSemesterDto { SemesterNumber = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.SemesterNumber);
    }

    [Fact]
    public void SemesterNumber_ProvidedAsZero_ShouldHaveError()
    {
        var dto = new UpdateSemesterDto { SemesterNumber = 0 };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SemesterNumber);
    }

    [Fact]
    public void SemesterNumber_ProvidedAsNine_ShouldHaveError()
    {
        var dto = new UpdateSemesterDto { SemesterNumber = 9 };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.SemesterNumber);
    }

    [Fact]
    public void SemesterNumber_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateSemesterDto { SemesterNumber = 4 };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.SemesterNumber);
    }
}
