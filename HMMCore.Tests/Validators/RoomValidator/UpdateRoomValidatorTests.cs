using FluentAssertions;
using HHMCore.Core.DTOs.Room;
using HHMCore.Core.Enums;
using HHMCore.Core.Validators.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMMCore.Tests.Validators.RoomValidator
{
    public class UpdateRoomValidatorTests
    {
        private readonly UpdateRoomValidator _validator = new();

        [Fact]
        public void EmptyDto_PassesValidation()
        {
            var result = _validator.Validate(new UpdateRoomDto());
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void RoomNumber_TooLong_WhenProvided_FailsValidation()
        {
            var dto = new UpdateRoomDto { RoomNumber = new string('A', 21) };
            _validator.Validate(dto).IsValid.Should().BeFalse();
        }

        [Fact]
        public void BuildingId_EmptyGuid_WhenProvided_FailsValidation()
        {
            var dto = new UpdateRoomDto { BuildingId = Guid.Empty };
            _validator.Validate(dto).IsValid.Should().BeFalse();
        }

        [Fact]
        public void Capacity_Zero_WhenProvided_FailsValidation()
        {
            var dto = new UpdateRoomDto { Capacity = 0 };
            _validator.Validate(dto).IsValid.Should().BeFalse();
        }

        [Fact]
        public void Capacity_Negative_WhenProvided_FailsValidation()
        {
            var dto = new UpdateRoomDto { Capacity = -1 };
            _validator.Validate(dto).IsValid.Should().BeFalse();
        }

        [Fact]
        public void RoomType_InvalidValue_WhenProvided_FailsValidation()
        {
            var dto = new UpdateRoomDto { RoomType = (RoomType)99 };
            _validator.Validate(dto).IsValid.Should().BeFalse();
        }

        [Fact]
        public void ValidDto_AllFieldsProvided_PassesValidation() //gonna look at this logic 
        {
            var dto = new UpdateRoomDto
            {
                RoomNumber = "B202",
                BuildingId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Capacity = 50,
                RoomType = RoomType.Lab,
                IsActive = false
            };
            _validator.Validate(dto).IsValid.Should().BeTrue();
        }
    }
}
