using FluentAssertions;
using HHMCore.Core.DTOs.Room;
using HHMCore.Core.Enums;
using HHMCore.Core.Validators.Room;

namespace HHMCore.Tests.Validators;

public class CreateRoomValidatorTests
{
    private readonly CreateRoomValidator _validator = new();

    private static CreateRoomDto ValidDto() => new()
    {
        RoomNumber = "A101",
        BuildingId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Capacity = 30,
        RoomType = RoomType.LectureHall
    };

    [Fact]
    public void ValidDto_PassesValidation()
    {
        var result = _validator.Validate(ValidDto());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RoomNumber_Empty_FailsValidation()
    {
        var dto = ValidDto();
        dto.RoomNumber = string.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void RoomNumber_TooLong_FailsValidation()
    {
        var dto = ValidDto();
        dto.RoomNumber = new string('A', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void BuildingId_Empty_FailsValidation()
    {
        var dto = ValidDto();
        dto.BuildingId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Capacity_Zero_FailsValidation()
    {
        var dto = ValidDto();
        dto.Capacity = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Capacity_Negative_FailsValidation()
    {
        var dto = ValidDto();
        dto.Capacity = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void RoomType_InvalidValue_FailsValidation()
    {
        var dto = ValidDto();
        dto.RoomType = (RoomType)99;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

