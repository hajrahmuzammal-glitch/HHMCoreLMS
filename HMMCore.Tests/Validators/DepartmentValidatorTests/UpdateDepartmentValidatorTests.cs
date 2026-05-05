using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Validators.Department;

namespace HHMCore.Tests.Validators;

public sealed class UpdateDepartmentValidatorTests
{
    private readonly UpdateDepartmentValidator _sut = new();

    // ── Rule 5: empty DTO must always pass ────────────────────────────────────

    [Fact]
    public void EmptyDto_AllNull_ShouldHaveNoErrors()
    {
        _sut.TestValidate(new UpdateDepartmentDto()).ShouldNotHaveAnyValidationErrors();
    }

    // ── Name ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Name_Null_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Name = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_WhitespaceOnly_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Name = "   " };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ProvidedAndExceeds100Chars_ShouldHaveError()
    {
        var dto = new UpdateDepartmentDto { Name = new string('A', 101) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Name = "Computer Engineering" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── Code ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Code_Null_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Code = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_WhitespaceOnly_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Code = "   " };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_ProvidedAndExceeds10Chars_ShouldHaveError()
    {
        var dto = new UpdateDepartmentDto { Code = new string('A', 11) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_ProvidedWithNumbers_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Code = "CS1" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Code_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Code = "CE" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    // ── Description ────────────────────────────────────────────────────────────

    [Fact]
    public void Description_Null_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Description = null };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_ProvidedAndExceeds500Chars_ShouldHaveError()
    {
        var dto = new UpdateDepartmentDto { Description = new string('A', 501) };
        _sut.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_ProvidedAndValid_ShouldNotHaveError()
    {
        var dto = new UpdateDepartmentDto { Description = "Valid description" };
        _sut.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
