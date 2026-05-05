using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using HHMCore.Core.DTOs.Building;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Mappings;
using HHMCore.Core.Services;
using Moq;

namespace HHMCore.Tests.Services;

public sealed class BuildingServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IGenericRepository<Building>> _repo;
    private readonly Mock<IGenericRepository<Room>> _roomRepo;
    private readonly IMapper _mapper;
    private readonly BuildingService _sut;

    public BuildingServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _repo = new Mock<IGenericRepository<Building>>();
        _roomRepo = new Mock<IGenericRepository<Room>>();

        _mapper = new MapperConfiguration(cfg =>
            cfg.AddProfile<BuildingMappingProfile>()).CreateMapper();

        _uow.Setup(u => u.Buildings).Returns(_repo.Object);
        _uow.Setup(u => u.Rooms).Returns(_roomRepo.Object);

        _sut = new BuildingService(_uow.Object, _mapper);
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static readonly Guid BuildingId = new("aaaaaaaa-0000-0000-0000-000000000001");

    private static Building Make(
        string name = "Block A",
        string? code = "BLK-A",
        bool isActive = true) => new()
        {
            Id = BuildingId,
            Name = name,
            Code = code,
            Description = "Main block",
            IsActive = isActive,
            Rooms = new List<Room>(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin@test.com"
        };

    private static Building MakeWithRooms(int roomCount) => new()
    {
        Id = BuildingId,
        Name = "Block A",
        Code = "BLK-A",
        IsActive = true,
        Rooms = Enumerable.Range(0, roomCount)
                              .Select(_ => new Room { Id = Guid.NewGuid() })
                              .ToList(),
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "admin@test.com"
    };

    private static CreateBuildingDto ValidCreate() => new()
    {
        Name = "Block A",
        Code = "BLK-A",
        Description = "Main block"
    };

    private static UpdateBuildingDto ValidUpdate() => new()
    {
        Name = "Block B",
        Code = "BLK-B"
    };

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Building>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(Make());

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Block A");
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsFailure()
    {
        // First ExistsAsync call = name check = true (duplicate)
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(true);

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("name already exists");
        _repo.Verify(r => r.AddAsync(It.IsAny<Building>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ReturnsFailure()
    {
        // First ExistsAsync = name check passes, second = code check fails
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false)   // name check
             .ReturnsAsync(true);   // code check

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("code already exists");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedByFromParameter()
    {
        Building? captured = null;
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Building>()))
             .Callback<Building>(b => captured = b)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(Make());

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        captured!.CreatedBy.Should().Be("admin@test.com");
    }
    [Fact]
    public async Task CreateAsync_TrimsWhitespaceFromName()
    {
        Building? captured = null;
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Building>()))
             .Callback<Building>(b => captured = b)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(Make());

        var dto = ValidCreate();
        dto.Name = "  Block A  ";
        await _sut.CreateAsync(dto, "admin@test.com");

        captured!.Name.Should().Be("Block A");
    }

    [Fact]
    public async Task CreateAsync_UppercasesCode()
    {
        Building? captured = null;
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Building>()))
             .Callback<Building>(b => captured = b)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(Make());

        var dto = ValidCreate();
        dto.Code = "blk-a";
        await _sut.CreateAsync(dto, "admin@test.com");

        captured!.Code.Should().Be("BLK-A");
    }

    [Fact]
    public async Task CreateAsync_CallsSaveChangesExactlyOnce()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Building>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(Make());

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        _repo.Setup(r => r.GetAllWithIncludesAsync(
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(new List<Building> { Make(), Make("Block B", "BLK-B") });

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsSuccessWithEmptyList()
    {
        _repo.Setup(r => r.GetAllWithIncludesAsync(
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(new List<Building>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    BuildingId,
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(Make());

        var result = await _sut.GetByIdAsync(BuildingId);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Block A");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync((Building?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetByIdAsync_RoomCountMappedFromRoomsCollection()
    {
        // Documents that RoomCount comes from Rooms.Count — verify mapping profile
        // includes: .ForMember(d => d.RoomCount, o => o.MapFrom(s => s.Rooms.Count))
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    BuildingId,
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(MakeWithRooms(3));

        var result = await _sut.GetByIdAsync(BuildingId);

        result.Success.Should().BeTrue();
        result.Data!.RoomCount.Should().Be(3);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsSuccessAndSavesOnce()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    BuildingId,
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(existing);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(BuildingId, ValidUpdate(), "admin@test.com");

        result.Success.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync((Building?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), ValidUpdate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsFailure()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    BuildingId,
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(existing);
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(true);   // name duplicate

        var result = await _sut.UpdateAsync(
            BuildingId, new UpdateBuildingDto { Name = "Block C" }, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateCode_ReturnsFailure()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    BuildingId,
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(existing);
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false)   // name check passes
             .ReturnsAsync(true);   // code check fails

        var result = await _sut.UpdateAsync(
            BuildingId, new UpdateBuildingDto { Name = "Block B", Code = "BLK-C" }, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmptyDto_RetainsAllExistingValues()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    BuildingId,
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(BuildingId, new UpdateBuildingDto(), "admin@test.com");

        result.Success.Should().BeTrue();
        existing.Name.Should().Be("Block A");
        existing.Code.Should().Be("BLK-A");
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedByAndUpdatedAt()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    BuildingId,
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(existing);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Building, bool>>>()))
             .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateAsync(BuildingId, ValidUpdate(), "admin@test.com");

        existing.UpdatedBy.Should().Be("admin@test.com");
        existing.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_IsActiveFalse_DeactivatesBuilding()
    {
        var existing = Make(isActive: true);
        _repo.Setup(r => r.GetByIdWithIncludesAsync(
                    BuildingId,
                    It.IsAny<Expression<Func<Building, object>>[]>()))
             .ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateAsync(BuildingId, new UpdateBuildingDto { IsActive = false }, "admin@test.com");

        existing.IsActive.Should().BeFalse();
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ValidBuildingNoRooms_SoftDeletesAndSaves()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(BuildingId)).ReturnsAsync(existing);
        _roomRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Room, bool>>>()))
                 .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(BuildingId, "admin@test.com");

        result.Success.Should().BeTrue();
        _repo.Verify(r => r.Delete(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Building?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _repo.Verify(r => r.Delete(It.IsAny<Building>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_HasRooms_ReturnsFailureWithoutDeleting()
    {
        _repo.Setup(r => r.GetByIdAsync(BuildingId)).ReturnsAsync(Make());
        _roomRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Room, bool>>>()))
                 .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(BuildingId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("rooms");
        _repo.Verify(r => r.Delete(It.IsAny<Building>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_SetsUpdatedByBeforeSave()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(BuildingId)).ReturnsAsync(existing);
        _roomRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Room, bool>>>()))
                 .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteAsync(BuildingId, "admin@test.com");

        existing.UpdatedBy.Should().Be("admin@test.com");
        existing.UpdatedAt.Should().NotBeNull();
    }
}
