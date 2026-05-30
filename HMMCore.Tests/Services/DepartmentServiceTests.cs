using System.Linq.Expressions;
using FluentAssertions;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Services;
using Moq;

namespace HHMCore.Tests.Services;

/// <summary>
/// Unit tests for <see cref="DepartmentService"/>.
/// All database calls are mocked — no real EF Core or SQL Server involved.
/// Each test covers one specific behaviour or failure case.
/// </summary>
public sealed class DepartmentServiceTests
{
    #region Setup

    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IGenericRepository<Department>> _repo;
    private readonly Mock<IGenericRepository<Teacher>> _teacherRepo;
    private readonly Mock<IGenericRepository<Student>> _studentRepo;
    private readonly Mock<IGenericRepository<Course>> _courseRepo;
    private readonly Mock<IGenericRepository<CourseAssignment>> _courseAssignmentRepo;
    private readonly DepartmentService _sut;

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

    #endregion

    #region Helpers

    // Fixed ID reused across tests to avoid magic Guid literals.
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

    /// <summary>
    /// Simulates no existing department matching either the Name or Code check.
    /// Used in happy-path create tests where duplicates should not interfere.
    /// </summary>
    private void SetupNoDuplicates() =>
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false)   // name check
             .ReturnsAsync(false);  // code check

    /// <summary>
    /// Simulates all dependency repos returning false — no teachers, students,
    /// courses, or course assignments linked to the department.
    /// Used in delete tests to isolate the specific dependency being tested.
    /// </summary>
    private void SetupAllDependentsEmpty()
    {
        _teacherRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Teacher, bool>>>()))
                    .ReturnsAsync(false);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(false);
        _courseRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Course, bool>>>()))
                   .ReturnsAsync(false);
        _courseAssignmentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
                             .ReturnsAsync(false);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        // Arrange
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Computer Science");
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsFailure()
    {
        // Arrange
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(true)    // name check fails
             .ReturnsAsync(false);  // code check never reached

        // Act
        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _repo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ReturnsFailure()
    {
        // Arrange
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false)  // name check passes
             .ReturnsAsync(true);  // code check fails

        // Act
        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
        _repo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UppercasesCode()
    {
        // Arrange — lowercase "cs" sent by caller
        Department? captured = null;
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>()))
             .Callback<Department>(d => captured = d)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var dto = ValidCreate();
        dto.Code = "cs";

        // Act
        await _sut.CreateAsync(dto, "admin@test.com");

        // Assert — stored value must always be uppercase regardless of input
        captured!.Code.Should().Be("CS");
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedByFromParameter()
    {
        // Arrange
        Department? captured = null;
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>()))
             .Callback<Department>(d => captured = d)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        // Assert
        captured!.CreatedBy.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task CreateAsync_SetsIsActiveTrue()
    {
        // Arrange
        Department? captured = null;
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>()))
             .Callback<Department>(d => captured = d)
             .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        // Assert — departments are active by default on creation
        captured!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_CallsSaveChangesExactlyOnce()
    {
        // Arrange
        SetupNoDuplicates();
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        // Assert — guards against accidental double-save introduced by future changes
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        // Arrange
        _repo.Setup(r => r.GetAllAsync())
             .ReturnsAsync(new List<Department> { Make(), Make("Electrical", "EE") });

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Department>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert — empty list is a valid success, not a failure
        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());

        // Act
        var result = await _sut.GetByIdAsync(DeptId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Computer Science");
        result.Data.Code.Should().Be("CS");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Department?)null);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsSuccessAndSavesOnce()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(DeptId, ValidUpdate(), "admin@test.com");

        // Assert
        result.Success.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync((Department?)null);

        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(), ValidUpdate(), "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsFailure()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(true);  // name duplicate check fires first

        var dto = new UpdateDepartmentDto { Name = "Electrical Engineering" };

        // Act
        var result = await _sut.UpdateAsync(DeptId, dto, "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("name already exists");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateCode_ReturnsFailure()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        _repo.SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false)  // name check passes
             .ReturnsAsync(true);  // code check fails

        var dto = new UpdateDepartmentDto { Name = "New Name", Code = "EE" };

        // Act
        var result = await _sut.UpdateAsync(DeptId, dto, "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("code already exists");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmptyDto_RetainsAllExistingValues()
    {
        // Arrange
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act — sending an empty DTO simulates a caller who provides no fields
        var result = await _sut.UpdateAsync(DeptId, new UpdateDepartmentDto(), "admin@test.com");

        // Assert — nothing should change when no fields are provided
        result.Success.Should().BeTrue();
        existing.Name.Should().Be("Computer Science");
        existing.Code.Should().Be("CS");
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedByAndUpdatedAt()
    {
        // Arrange
        var existing = Make();
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
             .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(DeptId, ValidUpdate(), "admin@test.com");

        // Assert
        existing.UpdatedBy.Should().Be("admin@test.com");
        existing.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_IsActiveProvided_UpdatesIsActive()
    {
        // Arrange
        var existing = Make();
        existing.IsActive = true;
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(DeptId, new UpdateDepartmentDto { IsActive = false }, "admin@test.com");

        // Assert
        existing.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_IsActiveNull_RetainsExistingIsActive()
    {
        // Arrange
        var existing = Make();
        existing.IsActive = true;
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act — IsActive not provided, existing value must be preserved
        await _sut.UpdateAsync(DeptId, new UpdateDepartmentDto { IsActive = null }, "admin@test.com");

        // Assert
        existing.IsActive.Should().BeTrue();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_HasTeachers_ReturnsFailure()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        SetupAllDependentsEmpty();
        _teacherRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Teacher, bool>>>()))
                    .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(DeptId, "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Teachers");
        _repo.Verify(r => r.Delete(It.IsAny<Department>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_HasStudents_ReturnsFailure()
    {
        // Arrange — all others empty, only students block the delete
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        SetupAllDependentsEmpty();
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(DeptId, "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Students");
        _repo.Verify(r => r.Delete(It.IsAny<Department>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_HasCourses_ReturnsFailure()
    {
        // Arrange — all others empty, only courses block the delete
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        SetupAllDependentsEmpty();
        _courseRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Course, bool>>>()))
                   .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(DeptId, "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Courses");
        _repo.Verify(r => r.Delete(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_HasCourseAssignments_ReturnsFailure()
    {
        // Arrange — CourseAssignment carries a denormalized DepartmentId,
        // so it must be checked independently even if courses are soft-deleted.
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(Make());
        SetupAllDependentsEmpty();
        _courseAssignmentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
                             .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(DeptId, "admin@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Course assignments");
        _repo.Verify(r => r.Delete(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_NoDependents_SoftDeletesAndStampsAudit()
    {
        // Arrange
        var dept = Make();
        _repo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(dept);
        SetupAllDependentsEmpty();
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteAsync(DeptId, "admin@test.com");

        // Assert — soft delete must stamp audit fields AND call Delete(), not hard delete
        result.Success.Should().BeTrue();
        dept.UpdatedBy.Should().Be("admin@test.com");
        dept.UpdatedAt.Should().NotBeNull();
        _repo.Verify(r => r.Delete(dept), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion
}
