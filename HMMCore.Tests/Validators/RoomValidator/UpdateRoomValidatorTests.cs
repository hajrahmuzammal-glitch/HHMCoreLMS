using FluentAssertions;
using FluentValidation.TestHelper;
using HHMCore.Core.DTOs.Room;
using HHMCore.Core.Enums;
using HHMCore.Core.Validators.Room;

namespace HHMCore.Tests.Validators.Room;

public class UpdateRoomValidatorTests
{
    private readonly UpdateRoomValidator _validator = new();

    private static readonly Guid ValidBuildingId =
        new Guid("11111111-1111-1111-1111-111111111111");

    // ── RoomNumber ────────────────────────────────────────────────────────────

    [Fact]
    public void RoomNumber_ProvidedButEmpty_FailsValidation()
    {
        // Sending an empty string means "I sent this field but gave it no value"
        // That is invalid even in an update
        var dto = new UpdateRoomDto { RoomNumber = string.Empty };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RoomNumber);
    }

    [Fact]
    public void RoomNumber_ProvidedAndExceedsMaxLength_FailsValidation()
    {
        var dto = new UpdateRoomDto { RoomNumber = new string('A', 21) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RoomNumber);
    }

    [Fact]
    public void RoomNumber_ProvidedAndValid_PassesValidation()
    {
        var dto = new UpdateRoomDto { RoomNumber = "Lab-3" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.RoomNumber);
    }

    // ── BuildingId ────────────────────────────────────────────────────────────

    [Fact]
    public void BuildingId_ProvidedAsEmptyGuid_FailsValidation()
    {
        // Providing Guid.Empty means the caller explicitly sent an invalid value
        var dto = new UpdateRoomDto { BuildingId = Guid.Empty };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BuildingId);
    }

    [Fact]
    public void BuildingId_ProvidedAndValid_PassesValidation()
    {
        var dto = new UpdateRoomDto { BuildingId = ValidBuildingId };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.BuildingId);
    }

    // ── Capacity ──────────────────────────────────────────────────────────────

    [Fact]
    public void Capacity_ProvidedAsZero_FailsValidation()
    {
        var dto = new UpdateRoomDto { Capacity = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    [Fact]
    public void Capacity_ProvidedAsNegative_FailsValidation()
    {
        var dto = new UpdateRoomDto { Capacity = -5 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    // ── RoomType ──────────────────────────────────────────────────────────────

    [Fact]
    public void RoomType_ProvidedAsInvalidEnum_FailsValidation()
    {
        var dto = new UpdateRoomDto { RoomType = (RoomType)99 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.RoomType);
    }

    // ── Empty dto — the most important test for UpdateValidator ───────────────

    [Fact]
    public void EmptyDto_AllFieldsNull_PassesValidation()
    {
        // A PATCH-style update sending nothing is valid
        // The service retains all existing values for null fields
        // The validator must not block this
        var dto = new UpdateRoomDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
