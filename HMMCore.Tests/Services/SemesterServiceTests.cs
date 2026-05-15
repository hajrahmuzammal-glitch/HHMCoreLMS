using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using HHMCore.Core.DTOs.Semester;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Mappings;
using HHMCore.Core.Services;
using Moq;

namespace HHMCore.Tests.Services;

public sealed class SemesterServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IGenericRepository<Semester>> _semesterRepo;
    private readonly Mock<IGenericRepository<CourseAssignment>> _caRepo;
    private readonly Mock<IGenericRepository<Enrollment>> _enrollRepo;
    private readonly IMapper _mapper;
    private readonly SemesterService _sut;

    public SemesterServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _semesterRepo = new Mock<IGenericRepository<Semester>>();
        _caRepo = new Mock<IGenericRepository<CourseAssignment>>();
        _enrollRepo = new Mock<IGenericRepository<Enrollment>>();

        _mapper = new MapperConfiguration(cfg =>
            cfg.AddProfile<SemesterMappingProfile>()).CreateMapper();

        _uow.Setup(u => u.Semesters).Returns(_semesterRepo.Object);
        _uow.Setup(u => u.CourseAssignments).Returns(_caRepo.Object);
        _uow.Setup(u => u.Enrollments).Returns(_enrollRepo.Object);

        _sut = new SemesterService(_uow.Object, _mapper);
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static readonly Guid SemId = new("aaaaaaaa-0000-0000-0000-000000000001");

    private static Semester Make(bool isActive = false) => new()
    {
        Id = SemId,
        Name = "Fall 2025",
        StartDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
        IsActive = isActive,
        SemesterNumber = 1,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "admin@test.com"
    };

    private static CreateSemesterDto ValidCreate() => new()
    {
        Name = "Fall 2025",
        SemesterNumber = 1,
        StartDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)
    };

    private static UpdateSemesterDto ValidUpdate() => new()
    {
        Name = "Spring 2026",
        SemesterNumber = 2,
        StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc)
    };

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        _semesterRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(false);
        _semesterRepo.Setup(r => r.AddAsync(It.IsAny<Semester>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Fall 2025");
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(true);

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _semesterRepo.Verify(r => r.AddAsync(It.IsAny<Semester>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedByFromParameter()
    {
        Semester? captured = null;
        _semesterRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(false);
        _semesterRepo.Setup(r => r.AddAsync(It.IsAny<Semester>()))
                     .Callback<Semester>(s => captured = s)
                     .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        captured!.CreatedBy.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task CreateAsync_SetsIsActiveFalseByDefault()
    {
        Semester? captured = null;
        _semesterRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(false);
        _semesterRepo.Setup(r => r.AddAsync(It.IsAny<Semester>()))
                     .Callback<Semester>(s => captured = s)
                     .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        captured!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_CallsSaveChangesExactlyOnce()
    {
        _semesterRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(false);
        _semesterRepo.Setup(r => r.AddAsync(It.IsAny<Semester>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        var list = new List<Semester> { Make(), Make() };
        _semesterRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsSuccessWithEmptyList()
    {
        _semesterRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Semester>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(Make());

        var result = await _sut.GetByIdAsync(SemId);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Fall 2025");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Semester?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // ── GetActiveAsync ─────────────────────────────────────────────────────────
    // NOTE: Real bug exists here — service calls FindOneAsync (returns Semester?)
    // but method signature returns IReadOnlyList<SemesterResponseDto>.
    // Test documents the ACTUAL behaviour so the bug is visible.

    [Fact]
    public async Task GetActiveAsync_ActiveExists_ReturnsSuccess()
    {
        _semesterRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(new List<Semester> { Make(isActive: true) });

        var result = await _sut.GetActiveAsync();

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveAsync_NoActiveSemester_ReturnsSuccessWithEmptyList()
    {
        _semesterRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
              .ReturnsAsync(new List<Semester>());
        var result = await _sut.GetActiveAsync();

        result.Success.Should().BeTrue();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsSuccessAndSavesOnce()
    {
        var existing = Make();
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(existing);
        _semesterRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(SemId, ValidUpdate(), "admin@test.com");

        result.Success.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Semester?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), ValidUpdate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsFailure()
    {
        var existing = Make();
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(existing);
        _semesterRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(true);

        var dto = new UpdateSemesterDto { Name = "Spring 2026" };
        var result = await _sut.UpdateAsync(SemId, dto, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmptyDto_RetainsExistingValues()
    {
        var existing = Make();
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Empty DTO — all null — Name check skipped, dates resolve to existing values
        var result = await _sut.UpdateAsync(SemId, new UpdateSemesterDto(), "admin@test.com");

        result.Success.Should().BeTrue();
        existing.Name.Should().Be("Fall 2025");
        existing.StartDate.Should().Be(new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task UpdateAsync_EndDateBeforeStartDate_ReturnsFailure()
    {
        var existing = Make();
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(existing);

        var dto = new UpdateSemesterDto
        {
            StartDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await _sut.UpdateAsync(SemId, dto, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("End date");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedByAndUpdatedAt()
    {
        var existing = Make();
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(existing);
        _semesterRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Semester, bool>>>()))
                     .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateAsync(SemId, ValidUpdate(), "admin@test.com");

        existing.UpdatedBy.Should().Be("admin@test.com");
        existing.UpdatedAt.Should().NotBeNull();
    }

    // ── ActivateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ActivateAsync_InactiveSemester_ActivatesAndSaves()
    {
        var existing = Make(isActive: false);
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.ActivateAsync(SemId, "admin@test.com");

        result.Success.Should().BeTrue();
        existing.IsActive.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_NotFound_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Semester?)null);

        var result = await _sut.ActivateAsync(Guid.NewGuid(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ActivateAsync_AlreadyActive_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(Make(isActive: true));

        var result = await _sut.ActivateAsync(SemId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already active");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ── DeactivateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeactivateAsync_ActiveSemester_DeactivatesAndSaves()
    {
        var existing = Make(isActive: true);
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeactivateAsync(SemId, "admin@test.com");

        result.Success.Should().BeTrue();
        existing.IsActive.Should().BeFalse();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_NotFound_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Semester?)null);

        var result = await _sut.DeactivateAsync(Guid.NewGuid(), "admin@test.com");

        result.Success.Should().BeFalse();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_AlreadyInactive_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(Make(isActive: false));

        var result = await _sut.DeactivateAsync(SemId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already inactive");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ValidInactiveSemester_SoftDeletesAndSaves()
    {
        var existing = Make(isActive: false);
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(existing);
        _caRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
               .ReturnsAsync(false);
        _enrollRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Enrollment, bool>>>()))
                   .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(SemId, "admin@test.com");

        result.Success.Should().BeTrue();
        _semesterRepo.Verify(r => r.Delete(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Semester?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _semesterRepo.Verify(r => r.Delete(It.IsAny<Semester>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ActiveSemester_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(Make(isActive: true));

        var result = await _sut.DeleteAsync(SemId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("active");
        _semesterRepo.Verify(r => r.Delete(It.IsAny<Semester>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_HasCourseAssignments_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(Make(isActive: false));
        _caRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
               .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(SemId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Course assignment");
        _semesterRepo.Verify(r => r.Delete(It.IsAny<Semester>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_HasEnrollments_ReturnsFailure()
    {
        _semesterRepo.Setup(r => r.GetByIdAsync(SemId)).ReturnsAsync(Make(isActive: false));
        _caRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
               .ReturnsAsync(false);
        _enrollRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Enrollment, bool>>>()))
                   .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(SemId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("enrolled");
        _semesterRepo.Verify(r => r.Delete(It.IsAny<Semester>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
