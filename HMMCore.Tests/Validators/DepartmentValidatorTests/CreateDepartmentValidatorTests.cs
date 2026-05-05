using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Validators.Department;

namespace HHMCore.Tests.Validators;

public sealed class CreateDepartmentValidatorTests
{
    private readonly CreateDepartmentValidator _sut = new();

    private static CreateDepartmentDto Valid() => new()
    {
        Name = "Computer Science",
        Code = "CS",
        Description = "CS department"
    };

    // ── Name ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Name_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.Name = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_TwoChars_ShouldHaveError()
    {
        var dto = Valid(); dto.Name = "AB";
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
    public void Name_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── Code ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Code_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.Code = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_Exceeds10Chars_ShouldHaveError()
    {
        var dto = Valid(); dto.Code = new string('A', 11);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_ContainsNumbers_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Code = "CS1";
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_ContainsSpecialChars_ShouldHaveError()
    {
        var dto = Valid(); dto.Code = "CS-1";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_Valid_ShouldNotHaveError()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    // ── Description ────────────────────────────────────────────────────────────

    [Fact]
    public void Description_Null_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Description = null;
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_Exceeds500Chars_ShouldHaveError()
    {
        var dto = Valid(); dto.Description = new string('A', 501);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_Exactly500Chars_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Description = new string('A', 500);
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // ── Full happy path ────────────────────────────────────────────────────────

    [Fact]
    public void ValidDto_ShouldHaveNoErrors()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }
}
