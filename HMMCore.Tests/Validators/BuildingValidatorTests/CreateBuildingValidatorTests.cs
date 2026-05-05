using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Building;
using HHMCore.Core.Validators.Building;

namespace HHMCore.Tests.Validators;

public sealed class CreateBuildingValidatorTests
{
    private readonly CreateBuildingValidator _sut = new();

    private static CreateBuildingDto Valid() => new()
    {
        Name = "Block A",
        Code = "BLK-A",
        Description = "Main teaching block"
    };

    // ── Name ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Name_Empty_ShouldHaveError()
    {
        var dto = Valid(); dto.Name = string.Empty;
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_TooShort_ShouldHaveError()
    {
        var dto = Valid(); dto.Name = "A";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_Exceeds100Chars_ShouldHaveError()
    {
        var dto = Valid(); dto.Name = new string('A', 101);
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_Exactly2Chars_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Name = "AB";
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
    public void Code_Null_ShouldHaveError()
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
    public void Code_Exactly10Chars_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Code = "BLK-123456".Substring(0, 10);
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }
    [Fact]
    public void Code_ContainsSpecialChars_ShouldHaveError()
    {
        var dto = Valid(); dto.Code = "BLK@A";
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_LettersNumbersAndHyphens_ShouldNotHaveError()
    {
        var dto = Valid(); dto.Code = "BLK-1A";
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
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
