using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Student;
using HHMCore.Core.Validators.Student;

namespace HHMCore.Tests.Validators;

public sealed class CreateStudentValidatorTests
{
    private readonly CreateStudentValidator _sut = new();

    private static CreateStudentDto Valid() => new()
    {
        FullName = "Ahmed Khan",
        Email = "student@test.com",
        Password = "Student@123",
        RollNumber = "CS-2025-001",
        DepartmentId = Guid.NewGuid(),
        CurrentSemesterNumber = 1,
        PhoneNumber = "03001234567",
        Address = "123 Main Street, Karachi",
        DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    // ── FullName ───────────────────────────────────────────────────────────────

    [Fact]
    public void FullName_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.FullName = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void FullName_TooShort_ShouldHaveError()
    {
        var dto = Valid(); dto.FullName = "AB";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void FullName_Exceeds100Chars_ShouldHaveError()
    {
        var dto = Valid(); dto.FullName = new string('A', 101);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void FullName_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    // ── Email ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Email_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.Email = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_InvalidFormat_ShouldHaveError()
    {
        var dto = Valid(); dto.Email = "not-an-email";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // ── Password ───────────────────────────────────────────────────────────────

    [Fact]
    public void Password_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.Password = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_TooShort_ShouldHaveError()
    {
        var dto = Valid(); dto.Password = "Ab1!";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_NoUppercase_ShouldHaveError()
    {
        var dto = Valid(); dto.Password = "student@123";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_NoSpecialChar_ShouldHaveError()
    {
        var dto = Valid(); dto.Password = "Student123";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    // ── RollNumber ─────────────────────────────────────────────────────────────

    [Fact]
    public void RollNumber_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.RollNumber = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.RollNumber);
    }

    [Fact]
    public void RollNumber_Exceeds20Chars_ShouldHaveError()
    {
        var dto = Valid(); dto.RollNumber = new string('A', 21);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.RollNumber);
    }

    [Fact]
    public void RollNumber_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.RollNumber);
    }

    // ── DepartmentId ───────────────────────────────────────────────────────────

    [Fact]
    public void DepartmentId_EmptyGuid_ShouldHaveError()
    {
        var dto = Valid(); dto.DepartmentId = Guid.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void DepartmentId_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.DepartmentId);
    }

    // ── CurrentSemesterNumber ──────────────────────────────────────────────────

    [Fact]
    public void CurrentSemesterNumber_Zero_ShouldHaveError()
    {
        var dto = Valid(); dto.CurrentSemesterNumber = 0;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.CurrentSemesterNumber);
    }

    [Fact]
    public void CurrentSemesterNumber_Nine_ShouldHaveError()
    {
        var dto = Valid(); dto.CurrentSemesterNumber = 9;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.CurrentSemesterNumber);
    }

    [Fact]
    public void CurrentSemesterNumber_Boundary1_ShouldNotHaveError()
    {
        var dto = Valid(); dto.CurrentSemesterNumber = 1;
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.CurrentSemesterNumber);
    }

    [Fact]
    public void CurrentSemesterNumber_Boundary8_ShouldNotHaveError()
    {
        var dto = Valid(); dto.CurrentSemesterNumber = 8;
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.CurrentSemesterNumber);
    }

    // ── PhoneNumber ────────────────────────────────────────────────────────────

    [Fact]
    public void PhoneNumber_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.PhoneNumber = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void PhoneNumber_InvalidFormat_ShouldHaveError()
    {
        var dto = Valid(); dto.PhoneNumber = "01234567890";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void PhoneNumber_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // ── DateOfBirth ────────────────────────────────────────────────────────────

    [Fact]
    public void DateOfBirth_TooYoung_ShouldHaveError()
    {
        var dto = Valid();
        dto.DateOfBirth = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_Before1950_ShouldHaveError()
    {
        var dto = Valid();
        dto.DateOfBirth = new DateTime(1949, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    // ── Address ────────────────────────────────────────────────────────────────

    [Fact]
    public void Address_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.Address = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Address_Exceeds250Chars_ShouldHaveError()
    {
        var dto = Valid(); dto.Address = new string('A', 251);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Address_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    // ── Full happy path ────────────────────────────────────────────────────────

    [Fact]
    public void ValidDto_ShouldHaveNoErrors()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }
}
