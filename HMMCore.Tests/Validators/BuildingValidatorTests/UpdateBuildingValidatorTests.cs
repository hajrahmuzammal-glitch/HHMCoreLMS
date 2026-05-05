using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Building;
using HHMCore.Core.Validators.Building;

namespace HHMCore.Tests.Validators;

public sealed class UpdateBuildingValidatorTests
{
    private readonly UpdateBuildingValidator _sut = new();

    // ── Rule 5: empty DTO must always pass ────────────────────────────────────

    [Fact]
    public void EmptyDto_AllNull_ShouldHaveNoErrors()
    {
        _sut.TestValidate(new UpdateBuildingDto()).ShouldNotHaveAnyValidationErrors();
    }

    // ── Name ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Name_Null_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Name = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }
    [Fact]
    public void Name_EmptyString_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Name = "" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ProvidedAndTooShort_ShouldHaveError()
    {
        var dto = new UpdateBuildingDto { Name = "A" };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ProvidedAndExceeds100Chars_ShouldHaveError()
    {
        var dto = new UpdateBuildingDto { Name = new string('A', 101) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Name = "Block B" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── Code ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Code_Null_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Code = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }
    [Fact]
    public void Code_EmptyString_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Code = "" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }
    [Fact]
    public void Code_ProvidedAndExceeds10Chars_ShouldHaveError()
    {
        var dto = new UpdateBuildingDto { Code = new string('A', 11) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_ProvidedWithInvalidChars_ShouldHaveError()
    {
        var dto = new UpdateBuildingDto { Code = "BLK@A" };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Code = "BLK-B" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    // ── Description ────────────────────────────────────────────────────────────

    [Fact]
    public void Description_Null_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Description = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Description);
    }
    [Fact]
    public void Description_EmptyString_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Description = "" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Description);
    }
    [Fact]
    public void Description_ProvidedAndExceeds500Chars_ShouldHaveError()
    {
        var dto = new UpdateBuildingDto { Description = new string('A', 501) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateBuildingDto { Description = "Updated description" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
