using System.Linq.Expressions;
using FluentAssertions;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Services;
using Moq;

namespace HHMCore.Tests.Services;

public sealed class DepartmentServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IGenericRepository<Department>> _repo;
    private readonly DepartmentService _sut;
    private readonly Mock<IGenericRepository<Teacher>> _teacherRepo;
    private readonly Mock<IGenericRepository<Student>> _studentRepo;
    private readonly Mock<IGenericRepository<Course>> _courseRepo;
    private readonly Mock<IGenericRepository<CourseAssignment>> _courseAssignmentRepo;

    public DepartmentServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _repo = new Mock<IGenericRepository<Department>>();

        _teacherRepo = new Mock<IGenericRepository<Teacher>>();
        _studentRepo = new Mock<IGenericRepository<Student>>();
        _courseRepo = new Mock<IGenericRepository<Course>>();
        _courseAssignmentRepo = new Mock<IGenericRepository<CourseAssignment>>();

        _uow.Setup(u => u.Departments).Returns(_repo.Object);
        _uow.Setup(u => u.Teachers).Returns(_teacherRepo.Object);
        _uow.Setup(u => u.Students).Returns(_studentRepo.Object);
        _uow.Setup(u => u.Courses).Returns(_courseRepo.Object);
        _uow.Setup(u => u.CourseAssignments).Returns(_courseAssignmentRepo.Object);

        _sut = new DepartmentService(_uow.Object);
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static readonly Guid DeptId = new("aaaaaaaa-0000-0000-0000-000000000001");

    private static Department Make(
        string name = "Computer Science",
        string code = "CS") => new()
        {
            Id = DeptId,
            Name = name,
            Code = code,
            Description = "CS department",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "admin@test.com"
        };

    private static CreateDepartmentDto ValidCreate() => new()
    {
        Name = "Computer Science",
        Code = "CS",
        Description = "CS department"
    };

    private static UpdateDepartmentDto ValidUpdate() => new()
    {
        Name = "Computer Engineering",
        Code = "CE"
    };
    private void SetupNoDuplicates() =>
    _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
         .ReturnsAsync(false)
         .ReturnsAsync(false);

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        // CreateAsync uses FindAsync(...).Any() — not ExistsAsync
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Computer Science");
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsFailure()
    {
        //Arrange
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(true)   
             .ReturnsAsync(false);  // code check passes
        //Act
        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        //Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _repo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ReturnsFailure()
    {
        //Arrange
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false)   // name check passes
             .ReturnsAsync(true);

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _repo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UppercasesCode()
    {
        Department? captured = null;
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>()))
             .Callback<Department>(d => captured = d)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var dto = ValidCreate();
        dto.Code = "cs";
        await _sut.CreateAsync(dto, "admin@test.com");

        captured!.Code.Should().Be("CS");
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedByFromParameter()
    {
        Department? captured = null;
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>()))
             .Callback<Department>(d => captured = d)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        captured!.CreatedBy.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task CreateAsync_SetsIsActiveTrue()
    {
        Department? captured = null;
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>()))
             .Callback<Department>(d => captured = d)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        captured!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_CallsSaveChangesExactlyOnce()
    {
        //Arrange
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        _repo.Setup(r => r.GetAllAsync())
             .ReturnsAsync(new List<Department> { Make(), Make("Electrical", "EE") });

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsSuccessWithEmptyList()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());

        var result = await _sut.GetByIdAsync(DeptId);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Computer Science");
        result.Data.Code.Should().Be("CS");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Department?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsSuccessAndSavesOnce()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(DeptId, ValidUpdate(), "admin@test.com");

        result.Success.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Department?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), ValidUpdate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsFailure()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(true);   // name duplicate check fires first

        var dto = new UpdateDepartmentDto { Name = "Electrical Engineering" };
        var result = await _sut.UpdateAsync(DeptId, dto, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("name already exists");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateCode_ReturnsFailure()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false)   // name check passes
             .ReturnsAsync(true);   // code check fails

        var dto = new UpdateDepartmentDto { Name = "New Name", Code = "EE" };
        var result = await _sut.UpdateAsync(DeptId, dto, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("code already exists");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmptyDto_RetainsAllExistingValues()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(DeptId, new UpdateDepartmentDto(), "admin@test.com");

        result.Success.Should().BeTrue();
        existing.Name.Should().Be("Computer Science");
        existing.Code.Should().Be("CS");
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedByAndUpdatedAt()
    {
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateAsync(DeptId, ValidUpdate(), "admin@test.com");

        existing.UpdatedBy.Should().Be("admin@test.com");
        existing.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_IsActiveProvided_UpdatesIsActive()
    {
        var existing = Make();
        existing.IsActive = true;
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var dto = new UpdateDepartmentDto { IsActive = false };
        await _sut.UpdateAsync(DeptId, dto, "admin@test.com");

        existing.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_IsActiveNull_RetainsExistingIsActive()
    {
        var existing = Make();
        existing.IsActive = true;
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateAsync(DeptId, new UpdateDepartmentDto { IsActive = null }, "admin@test.com");

        existing.IsActive.Should().BeTrue();
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────
    // IMPROVEMENT FLAGGED: DeleteAsync has no dependent-record guard.
    // Deleting a department that has courses/students/teachers will orphan data.
    // Add checks for _unitOfWork.Courses.ExistsAsync, Students, Teachers before Delete.
    // Scheduled for: Session 12 refactor phase.

    [Fact]
    public async Task DeleteAsync_HasTeachers_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        _teacherRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Teacher, bool>>>()))
                    .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(DeptId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Teachers");
        _repo.Verify(r => r.Delete(It.IsAny<Department>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_HasStudents_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        _teacherRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Teacher, bool>>>()))
                    .ReturnsAsync(false);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(DeptId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Students");
        _repo.Verify(r => r.Delete(It.IsAny<Department>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
