using FluentValidation.TestHelper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Student;
using HHMCore.Core.Validators.Student;

namespace HHMCore.Tests.Validators;

public sealed class UpdateStudentValidatorTests
{
    private readonly UpdateStudentValidator _sut = new();

    // ── Rule 5: empty DTO must always pass ────────────────────────────────────

    [Fact]
    public void EmptyDto_AllNull_ShouldHaveNoErrors()
    {
        _sut.TestValidate(new UpdateStudentDto()).ShouldNotHaveAnyValidationErrors();
    }

    // ── FullName ───────────────────────────────────────────────────────────────

    [Fact]
    public void FullName_Null_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { FullName = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void FullName_ProvidedAndTooShort_ShouldHaveError()
    {
        var dto = new UpdateStudentDto { FullName = "AB" };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void FullName_ProvidedAndExceeds100Chars_ShouldHaveError()
    {
        var dto = new UpdateStudentDto { FullName = new string('A', 101) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void FullName_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { FullName = "Ahmed Khan" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    // ── PhoneNumber ────────────────────────────────────────────────────────────

    [Fact]
    public void PhoneNumber_Null_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { PhoneNumber = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void PhoneNumber_ProvidedAndInvalidFormat_ShouldHaveError()
    {
        var dto = new UpdateStudentDto { PhoneNumber = "01234567890" };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void PhoneNumber_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { PhoneNumber = "03001234567" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // ── DateOfBirth ────────────────────────────────────────────────────────────

    [Fact]
    public void DateOfBirth_Null_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { DateOfBirth = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_ProvidedAndTooYoung_ShouldHaveError()
    {
        var dto = new UpdateStudentDto { DateOfBirth = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_ProvidedAndBefore1950_ShouldHaveError()
    {
        var dto = new UpdateStudentDto
        {
            DateOfBirth = new DateTime(1949, 12, 31, 0, 0, 0, DateTimeKind.Utc)
        };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto
        {
            DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    // ── CurrentSemesterNumber ──────────────────────────────────────────────────

    [Fact]
    public void CurrentSemesterNumber_Null_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { CurrentSemesterNumber = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.CurrentSemesterNumber);
    }

    [Fact]
    public void CurrentSemesterNumber_ProvidedAsZero_ShouldHaveError()
    {
        var dto = new UpdateStudentDto { CurrentSemesterNumber = 0 };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.CurrentSemesterNumber);
    }

    [Fact]
    public void CurrentSemesterNumber_ProvidedAsNine_ShouldHaveError()
    {
        var dto = new UpdateStudentDto { CurrentSemesterNumber = 9 };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.CurrentSemesterNumber);
    }

    [Fact]
    public void CurrentSemesterNumber_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { CurrentSemesterNumber = 4 };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.CurrentSemesterNumber);
    }

    // ── DepartmentId ───────────────────────────────────────────────────────────

    [Fact]
    public void DepartmentId_Null_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { DepartmentId = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void DepartmentId_ProvidedAsEmptyGuid_ShouldHaveError()
    {
        var dto = new UpdateStudentDto { DepartmentId = Guid.Empty };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void DepartmentId_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { DepartmentId = Guid.NewGuid() };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.DepartmentId);
    }
    // ── Status ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Status_Null_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { Status = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Status_ProvidedAsInvalidValue_ShouldHaveError()
    {
        var dto = new UpdateStudentDto { Status = "Banana" };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Status_ProvidedAsActive_ShouldNotHaveError()
    {
        var dto = new UpdateStudentDto { Status = StudentStatus.Active };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Status);
    }
}
