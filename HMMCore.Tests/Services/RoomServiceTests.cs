using AutoMapper;
using FluentAssertions;
using Moq;
using HHMCore.Core.DTOs.Room;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Services;
using HHMCore.Tests.Helpers;
using System.Linq.Expressions;

namespace HHMCore.Tests.Services;

public class RoomServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IGenericRepository<Room>> _roomRepoMock;
    private readonly Mock<IGenericRepository<Building>> _buildingRepoMock;
    private readonly IMapper _mapper;
    private readonly RoomService _sut;

    // Fixed Guids so tests are deterministic — never use random Guids in tests
    private static readonly Guid BuildingAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UnknownBldId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public RoomServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _roomRepoMock = new Mock<IGenericRepository<Room>>();
        _buildingRepoMock = new Mock<IGenericRepository<Building>>();
        _mapper = MapperFactory.Create();

        _uowMock.Setup(u => u.Rooms).Returns(_roomRepoMock.Object);
        _uowMock.Setup(u => u.Buildings).Returns(_buildingRepoMock.Object);

        _sut = new RoomService(_uowMock.Object, _mapper);
    }

    // ── Shared test data ──────────────────────────────────────────────────────

    private static Building MainBlock() => new()
    {
        Id = BuildingAId,
        Name = "Main Block"
    };

    private static Room SavedRoom(Guid buildingId) => new()
    {
        Id = Guid.NewGuid(),
        RoomNumber = "A101",
        Capacity = 30,
        BuildingId = buildingId,
        Building = MainBlock()
    };

    private static CreateRoomDto ValidDto() => new()
    {
        RoomNumber = "A101",
        Capacity = 30,
        BuildingId = BuildingAId
    };

    // ── HAPPY PATH ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var dto = ValidDto();

        _buildingRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
            .ReturnsAsync(true);                        // building exists

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Room, bool>>>()))
            .ReturnsAsync(false);                       // no duplicate

        _roomRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(SavedRoom(BuildingAId));

        // Act
        var result = await _sut.CreateAsync(dto, "admin@test.com");

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.BuildingName.Should().Be("Main Block");
        result.Data.RoomNumber.Should().Be("A101");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesExactlyOnce()
    {
        // Arrange
        var dto = ValidDto();

        _buildingRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
            .ReturnsAsync(true);

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Room, bool>>>()))
            .ReturnsAsync(false);

        _roomRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(SavedRoom(BuildingAId));

        // Act
        await _sut.CreateAsync(dto, "admin@test.com");

        // Assert — DB must be touched exactly once
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _roomRepoMock.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Once);
    }

    // ── SAD PATHS ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_BuildingNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateRoomDto
        {
            RoomNumber = "B202",
            Capacity = 20,
            BuildingId = UnknownBldId
        };

        _buildingRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
            .ReturnsAsync(false);                       // building does not exist

        // Act
        var result = await _sut.CreateAsync(dto, "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Building");
        _roomRepoMock.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateRoom_ReturnsConflict()
    {
        // Arrange
        var dto = ValidDto();

        _buildingRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
            .ReturnsAsync(true);                        // building exists

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Room, bool>>>()))
            .ReturnsAsync(true);                        // duplicate room exists

        // Act
        var result = await _sut.CreateAsync(dto, "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _roomRepoMock.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
    // ── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_RoomsExist_ReturnsList()
    {
        _roomRepoMock
            .Setup(r => r.GetAllWithIncludesAsync(It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(new List<Room> { SavedRoom(BuildingAId) });

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_NoRooms_ReturnsEmptyList()
    {
        _roomRepoMock
            .Setup(r => r.GetAllWithIncludesAsync(It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(new List<Room>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    // ── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsRoom()
    {
        var roomId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                roomId,
                It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(SavedRoom(BuildingAId));

        var result = await _sut.GetByIdAsync(roomId);

        result.Success.Should().BeTrue();
        result.Data!.BuildingName.Should().Be("Main Block");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFailure()
    {
        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync((Room?)null);

        var result = await _sut.GetByIdAsync(Guid.Parse("33333333-3333-3333-3333-333333333333"));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync((Room?)null);

        var result = await _sut.UpdateAsync(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            new UpdateRoomDto { Capacity = 50 },
            "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateAsync_BuildingNotFound_ReturnsFailure()
    {
        var roomId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var unknownBldId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                roomId,
                It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(SavedRoom(BuildingAId));

        _buildingRepoMock
            .Setup(r => r.GetByIdAsync(unknownBldId))
            .ReturnsAsync((Building?)null);

        var result = await _sut.UpdateAsync(
            roomId,
            new UpdateRoomDto { BuildingId = unknownBldId },
            "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Building");
    }

    [Fact]
    public async Task UpdateAsync_DuplicateRoomNumber_ReturnsFailure()
    {
        var roomId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                roomId,
                It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(SavedRoom(BuildingAId));

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Room, bool>>>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateAsync(
            roomId,
            new UpdateRoomDto { RoomNumber = "A101" },
            "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task UpdateAsync_ValidChange_SavesOnce()
    {
        var roomId = Guid.Parse("88888888-8888-8888-8888-888888888888");

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(
                roomId,
                It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(SavedRoom(BuildingAId));

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Room, bool>>>()))
            .ReturnsAsync(false);

        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(
            roomId,
            new UpdateRoomDto { Capacity = 60 },
            "admin@test.com");

        result.Success.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFailure()
    {
        _roomRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Room?)null);

        var result = await _sut.DeleteAsync(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "admin@test.com");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_HasAssignments_ReturnsFailure()
    {
        var roomId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        _roomRepoMock
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(SavedRoom(BuildingAId));

        _uowMock.Setup(u => u.CourseAssignments)
            .Returns(new Mock<IGenericRepository<CourseAssignment>>().Object);

        var caRepoMock = new Mock<IGenericRepository<CourseAssignment>>();
        caRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
            .ReturnsAsync(true);
        _uowMock.Setup(u => u.CourseAssignments).Returns(caRepoMock.Object);

        var result = await _sut.DeleteAsync(roomId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("assigned");
    }

    [Fact]
    public async Task DeleteAsync_ValidRoom_SoftDeletesAndSavesOnce()
    {
        var roomId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var room = SavedRoom(BuildingAId);

        _roomRepoMock
            .Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        var caRepoMock = new Mock<IGenericRepository<CourseAssignment>>();
        caRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
            .ReturnsAsync(false);
        _uowMock.Setup(u => u.CourseAssignments).Returns(caRepoMock.Object);

        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(roomId, "admin@test.com");

        result.Success.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _roomRepoMock.Verify(r => r.Delete(room), Times.Once);
    }
}