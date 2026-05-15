using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Auth;
using HHMCore.Core.Validators.Auth;

namespace HHMCore.Tests.Validators;

public sealed class LoginValidatorTests
{
    private readonly LoginValidator _sut = new();

    private static LoginDto Valid() => new()
    {
        Email = "admin@test.com",
        Password = "Admin@123"
    };

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
    public void Password_SimplePassword_ShouldNotHaveError()
    {
        // Login only checks NotEmpty — simple passwords pass
        var dto = Valid(); dto.Password = "abc";
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    // ── Full happy path ────────────────────────────────────────────────────────

    [Fact]
    public void ValidDto_ShouldHaveNoErrors()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }
}
