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
    private readonly IMapper _mapper;
    private readonly RoomService _sut;

    public RoomServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _roomRepoMock = new Mock<IGenericRepository<Room>>();
        _mapper = MapperFactory.Create();

        _uowMock.Setup(u => u.Rooms).Returns(_roomRepoMock.Object);

        _sut = new RoomService(_uowMock.Object, _mapper);
    }

    // ── HAPPY PATH ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange

        var building = new Building
        { 
            Id = Guid.NewGuid(),
            Name = "Main Block"
        };

        var dto = new CreateRoomDto
        {
            RoomNumber = "A101",
            Capacity = 30,
            BuildingId = building.Id
        };

        var savedRoom = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = "A101",
            Capacity = 30,
            BuildingId = building.Id ,
            Building = building
        };

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Room, bool>>>()))
            .ReturnsAsync(false);

        _roomRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(savedRoom);

        // Act
        var result = await _sut.CreateAsync(dto, "admin@test.com");

        // Assert
        result.Success.Should().BeTrue();
        result.Data.BuildingName.Should().Be("Main Block"); // Bug 1 — this was null before fix
    }

    // ── SAD PATHS ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DuplicateRoom_ReturnsConflict()
    {
        // Arrange
        var dto = new CreateRoomDto
        {
            RoomNumber = "A101",
            Capacity = 30,
            BuildingId = 1
        };

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Room, bool>>>()))
            .ReturnsAsync(true); // duplicate exists

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("already exists");

        _roomRepoMock.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_BuildingNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateRoomDto
        {
            RoomNumber = "B202",
            Capacity = 20,
            BuildingId = 999 // non-existent
        };

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Room, bool>>>()))
            .ReturnsAsync(false);

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<Room, object>>[]>()))
            .ReturnsAsync((Room?)null); // building not found

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ── EDGE CASES ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_EmptyRoomNumber_ReturnsFailure()
    {
        var dto = new CreateRoomDto
        {
            RoomNumber = "",
            Capacity = 30,
            BuildingId = 1
        };

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        _roomRepoMock.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ZeroCapacity_ReturnsFailure()
    {
        var dto = new CreateRoomDto
        {
            RoomNumber = "C303",
            Capacity = 0,
            BuildingId = 1
        };

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        _roomRepoMock.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NegativeCapacity_ReturnsFailure()
    {
        var dto = new CreateRoomDto
        {
            RoomNumber = "D404",
            Capacity = -5,
            BuildingId = 1
        };

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        _roomRepoMock.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Never);
    }

    // ── VERIFY SAVE CALLED EXACTLY ONCE ON SUCCESS ───────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesExactlyOnce()
    {
        // Arrange
        var dto = new CreateRoomDto
        {
            RoomNumber = "E505",
            Capacity = 40,
            BuildingId = 2
        };

        var building = new Building { Id = 2, Name = "Science Block" };

        var savedRoom = new Room
        {
            Id = 2,
            RoomNumber = "E505",
            Capacity = 40,
            BuildingId = 2,
            Building = building
        };

        _roomRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Room, bool>>>()))
            .ReturnsAsync(false);

        _roomRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Room>()))
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _roomRepoMock
            .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<Room, object>>[]>()))
            .ReturnsAsync(savedRoom);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _roomRepoMock.Verify(r => r.AddAsync(It.IsAny<Room>()), Times.Once);
    }
}