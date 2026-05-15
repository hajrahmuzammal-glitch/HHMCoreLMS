using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Room;
using HHMCore.Core.Enums;
using HHMCore.Core.Validators.Room;

namespace HHMCore.Tests.Validators.Room;

public class CreateRoomValidatorTests
{
    private readonly CreateRoomValidator _validator = new();

    private static readonly Guid ValidBuildingId =
        new Guid("11111111-1111-1111-1111-111111111111");

    private static CreateRoomDto ValidDto() => new()
    {
        RoomNumber = "101",
        BuildingId = ValidBuildingId,
        Capacity = 30,
        RoomType = RoomType.LectureHall
    };

    // ── RoomNumber ────────────────────────────────────────────────────────────

    [Fact]
    public void RoomNumber_Empty_FailsValidation()
    {
        var dto = ValidDto();
        dto.RoomNumber = string.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RoomNumber);
    }

    [Fact]
    public void RoomNumber_ExceedsMaxLength_FailsValidation()
    {
        var dto = ValidDto();
        dto.RoomNumber = new string('A', 21); // 21 chars — limit is 20

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RoomNumber);
    }

    // ── BuildingId ────────────────────────────────────────────────────────────

    [Fact]
    public void BuildingId_EmptyGuid_FailsValidation()
    {
        var dto = ValidDto();
        dto.BuildingId = Guid.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BuildingId);
    }

    // ── Capacity ──────────────────────────────────────────────────────────────

    [Fact]
    public void Capacity_Zero_FailsValidation()
    {
        var dto = ValidDto();
        dto.Capacity = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    [Fact]
    public void Capacity_Negative_FailsValidation()
    {
        var dto = ValidDto();
        dto.Capacity = -1;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    // ── RoomType ──────────────────────────────────────────────────────────────

    [Fact]
    public void RoomType_InvalidEnumValue_FailsValidation()
    {
        var dto = ValidDto();
        dto.RoomType = (RoomType)99; // not a valid enum member

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RoomType);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void ValidDto_AllFieldsCorrect_PassesValidation()
    {
        var dto = ValidDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
