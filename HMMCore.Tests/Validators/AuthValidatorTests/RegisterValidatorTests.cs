using FluentValidation.TestHelper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Auth;
using HHMCore.Core.Validators.Auth;

namespace HHMCore.Tests.Validators;

public sealed class RegisterValidatorTests
{
    private readonly RegisterValidator _sut = new();

    private static RegisterDto Valid() => new()
    {
        FullName = "Test Admin",
        Email = "admin@test.com",
        Password = "Admin@123",
        ConfirmPassword = "Admin@123",
        Role = AppRoles.Admin
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
        var dto = Valid(); dto.FullName = "A";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void FullName_Exceeds100Chars_ShouldHaveError()
    {
        var dto = Valid(); dto.FullName = new string('A', 101);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void FullName_Exactly2Chars_ShouldNotHaveError()
    {
        var dto = Valid(); dto.FullName = "AB";
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.FullName);
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
        var dto = Valid(); dto.Password = "admin@123";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_NoLowercase_ShouldHaveError()
    {
        var dto = Valid(); dto.Password = "ADMIN@123";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_NoNumber_ShouldHaveError()
    {
        var dto = Valid(); dto.Password = "Admin@abc";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_NoSpecialChar_ShouldHaveError()
    {
        var dto = Valid(); dto.Password = "Admin1234";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Exactly8Chars_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Password = "Admin@1a";
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Password);
    }
    // ── ConfirmPassword ────────────────────────────────────────────────────────

    [Fact]
    public void ConfirmPassword_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.ConfirmPassword = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void ConfirmPassword_DoesNotMatchPassword_ShouldHaveError()
    {
        var dto = Valid(); dto.ConfirmPassword = "Different@123";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void ConfirmPassword_MatchesPassword_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    // ── Role ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Role_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.Role = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Role_InvalidValue_ShouldHaveError()
    {
        var dto = Valid(); dto.Role = "SuperAdmin";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Role_Admin_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Role = AppRoles.Admin;
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Role_Teacher_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Role = AppRoles.Teacher;
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Role_Student_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Role = AppRoles.Student;
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    // ── Full happy path ────────────────────────────────────────────────────────

    [Fact]
    public void ValidDto_ShouldHaveNoErrors()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }
}
