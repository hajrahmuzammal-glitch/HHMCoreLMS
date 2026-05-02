using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Student;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Mappings;
using HHMCore.Core.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace HHMCore.Tests.Services;

public sealed class StudentServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IGenericRepository<Student>> _studentRepo;
    private readonly Mock<IGenericRepository<Department>> _deptRepo;
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly Mock<IEmailService> _emailService;
    private readonly IMapper _mapper;
    private readonly StudentService _sut;

    public StudentServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _studentRepo = new Mock<IGenericRepository<Student>>();
        _deptRepo = new Mock<IGenericRepository<Department>>();
        _emailService = new Mock<IEmailService>();

        var store = new Mock<IUserStore<AppUser>>();
        _userManager = new Mock<UserManager<AppUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mapper = new MapperConfiguration(cfg =>
            cfg.AddProfile<StudentMappingProfile>()).CreateMapper();

        _uow.Setup(u => u.Students).Returns(_studentRepo.Object);
        _uow.Setup(u => u.Departments).Returns(_deptRepo.Object);

        // Email is fire-and-forget — always succeeds silently
        _emailService
            .Setup(e => e.SendCredentialsAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _sut = new StudentService(
            _uow.Object, _mapper, _userManager.Object, _emailService.Object);
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static readonly Guid StudentId = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid DeptId = new("bbbbbbbb-0000-0000-0000-000000000002");

    private static AppUser MakeAppUser() => new()
    {
        Id = "user-001",
        Email = "student@test.com",
        UserName = "student@test.com",
        FullName = "Ahmed Khan"
    };

    private static Department MakeDept() => new()
    {
        Id = DeptId,
        Name = "Computer Science",
        Code = "CS"
    };

    private static Student MakeStudent() => new()
    {
        Id = StudentId,
        UserId = "user-001",
        User = MakeAppUser(),
        RollNumber = "CS-2025-001",
        DepartmentId = DeptId,
        Department = MakeDept(),
        CurrentSemesterNumber = 1,
        Status = StudentStatus.Active,
        PhoneNumber = "03001234567",
        Address = "123 Main St, Karachi",
        DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        CreatedBy = "admin@test.com"
    };

    private static CreateStudentDto ValidCreate() => new()
    {
        FullName = "Ahmed Khan",
        Email = "student@test.com",
        Password = "Student@123",
        RollNumber = "CS-2025-001",
        DepartmentId = DeptId,
        CurrentSemesterNumber = 1,
        PhoneNumber = "03001234567",
        Address = "123 Main St, Karachi",
        DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private static UpdateStudentDto ValidUpdate() => new()
    {
        PhoneNumber = "03009876543",
        Address = "456 New St, Lahore"
    };

    // shared setup for happy-path create
    private void SetupCreateHappyPath(Student? returnAfterSave = null)
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(false);
        _deptRepo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(MakeDept());
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), AppRoles.Student))
                    .ReturnsAsync(IdentityResult.Success);
        _studentRepo.Setup(r => r.AddAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _studentRepo.Setup(r => r.GetByIdWithIncludesAsync(
                         It.IsAny<Guid>(),
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(returnAfterSave ?? MakeStudent());
    }

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccess()
    {
        SetupCreateHappyPath();

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.RollNumber.Should().Be("CS-2025-001");
    }

    [Fact]
    public async Task CreateAsync_EmailAlreadyExists_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(MakeAppUser());

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("email");
        _studentRepo.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateRollNumber_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(true);

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("roll number");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DepartmentNotFound_ReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(false);
        _deptRepo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync((Department?)null);

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Department");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_IdentityCreateFails_ReturnsFailureWithoutSaving()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(false);
        _deptRepo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(MakeDept());
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Failed(
                        new IdentityError { Description = "Password too weak." }));

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        _studentRepo.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RoleAssignmentFails_DeletesUserAndReturnsFailure()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(false);
        _deptRepo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(MakeDept());
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), AppRoles.Student))
                    .ReturnsAsync(IdentityResult.Failed(
                        new IdentityError { Description = "Role not found." }));
        _userManager.Setup(m => m.DeleteAsync(It.IsAny<AppUser>()))
                    .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        result.Success.Should().BeFalse();
        _userManager.Verify(m => m.DeleteAsync(It.IsAny<AppUser>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NormalizesEmailToLowercase()
    {
        AppUser? captured = null;
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(false);
        _deptRepo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(MakeDept());
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .Callback<AppUser, string>((u, _) => captured = u)
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), AppRoles.Student))
                    .ReturnsAsync(IdentityResult.Success);
        _studentRepo.Setup(r => r.AddAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _studentRepo.Setup(r => r.GetByIdWithIncludesAsync(
                         It.IsAny<Guid>(),
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(MakeStudent());

        var dto = ValidCreate();
        dto.Email = "STUDENT@TEST.COM";
        await _sut.CreateAsync(dto, "admin@test.com");

        captured!.Email.Should().Be("student@test.com");
    }

    [Fact]
    public async Task CreateAsync_NormalizesRollNumberToUppercase()
    {
        Student? captured = null;
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser?)null);
        _studentRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Student, bool>>>()))
                    .ReturnsAsync(false);
        _deptRepo.Setup(r => r.GetByIdAsync(DeptId)).ReturnsAsync(MakeDept());
        _userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                    .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), AppRoles.Student))
                    .ReturnsAsync(IdentityResult.Success);
        _studentRepo.Setup(r => r.AddAsync(It.IsAny<Student>()))
                    .Callback<Student>(s => captured = s)
                    .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _studentRepo.Setup(r => r.GetByIdWithIncludesAsync(
                         It.IsAny<Guid>(),
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(MakeStudent());

        var dto = ValidCreate();
        dto.RollNumber = "cs-2025-001";
        await _sut.CreateAsync(dto, "admin@test.com");

        captured!.RollNumber.Should().Be("CS-2025-001");
    }

    [Fact]
    public async Task CreateAsync_AssignsStudentRole()
    {
        SetupCreateHappyPath();

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        _userManager.Verify(
            m => m.AddToRoleAsync(It.IsAny<AppUser>(), AppRoles.Student), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CallsSaveChangesExactlyOnce()
    {
        SetupCreateHappyPath();

        await _sut.CreateAsync(ValidCreate(), "admin@test.com");

        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsMappedList()
    {
        _studentRepo.Setup(r => r.GetAllWithIncludesAsync(
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(new List<Student> { MakeStudent() });

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsSuccessWithEmptyList()
    {
        _studentRepo.Setup(r => r.GetAllWithIncludesAsync(
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(new List<Student>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        _studentRepo.Setup(r => r.GetByIdWithIncludesAsync(
                         StudentId,
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(MakeStudent());

        var result = await _sut.GetByIdAsync(StudentId);

        result.Success.Should().BeTrue();
        result.Data!.RollNumber.Should().Be("CS-2025-001");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFailure()
    {
        _studentRepo.Setup(r => r.GetByIdWithIncludesAsync(
                         It.IsAny<Guid>(),
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync((Student?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // ── GetMeAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMeAsync_ValidUserId_ReturnsProfile()
    {
        _studentRepo.Setup(r => r.FindOneWithIncludesAsync(
                         It.IsAny<Expression<Func<Student, bool>>>(),
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(MakeStudent());

        var result = await _sut.GetMeAsync("user-001");

        result.Success.Should().BeTrue();
        result.Data!.UserId.Should().Be("user-001");
    }

    [Fact]
    public async Task GetMeAsync_ProfileNotFound_ReturnsFailure()
    {
        _studentRepo.Setup(r => r.FindOneWithIncludesAsync(
                         It.IsAny<Expression<Func<Student, bool>>>(),
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync((Student?)null);

        var result = await _sut.GetMeAsync("user-999");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ValidDto_ReturnsSuccessAndSavesOnce()
    {
        var existing = MakeStudent();
        _studentRepo.SetupSequence(r => r.GetByIdWithIncludesAsync(
                         StudentId,
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(existing)   // first call — fetch to update
                    .ReturnsAsync(existing);  // second call — re-fetch for response
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(StudentId, ValidUpdate(), "admin@test.com");

        result.Success.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFailure()
    {
        _studentRepo.Setup(r => r.GetByIdWithIncludesAsync(
                         It.IsAny<Guid>(),
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync((Student?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), ValidUpdate(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DepartmentIdProvided_DepartmentNotFound_ReturnsFailure()
    {
        var existing = MakeStudent();
        _studentRepo.Setup(r => r.GetByIdWithIncludesAsync(
                         StudentId,
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(existing);
        _deptRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync((Department?)null);

        var dto = new UpdateStudentDto { DepartmentId = Guid.NewGuid() };
        var result = await _sut.UpdateAsync(StudentId, dto, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Department");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NullPhoneNumber_RetainsExistingPhoneNumber()
    {
        var existing = MakeStudent();
        existing.PhoneNumber = "03009999999";

        _studentRepo.SetupSequence(r => r.GetByIdWithIncludesAsync(
                         StudentId,
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(existing)
                    .ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateAsync(StudentId, new UpdateStudentDto { PhoneNumber = null }, "admin@test.com");

        existing.PhoneNumber.Should().Be("03009999999");
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedByAndUpdatedAt()
    {
        var existing = MakeStudent();
        _studentRepo.SetupSequence(r => r.GetByIdWithIncludesAsync(
                         StudentId,
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(existing)
                    .ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateAsync(StudentId, ValidUpdate(), "admin@test.com");

        existing.UpdatedBy.Should().Be("admin@test.com");
        existing.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_FullNameProvided_UpdatesViaUserManager()
    {
        var existing = MakeStudent();
        var appUser = MakeAppUser();

        _studentRepo.SetupSequence(r => r.GetByIdWithIncludesAsync(
                         StudentId,
                         It.IsAny<Expression<Func<Student, object>>[]>()))
                    .ReturnsAsync(existing)
                    .ReturnsAsync(existing);
        _userManager.Setup(m => m.FindByIdAsync(existing.UserId)).ReturnsAsync(appUser);
        _userManager.Setup(m => m.UpdateAsync(appUser)).ReturnsAsync(IdentityResult.Success);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.UpdateAsync(StudentId, new UpdateStudentDto { FullName = "New Name" }, "admin@test.com");

        _userManager.Verify(m => m.UpdateAsync(appUser), Times.Once);
        appUser.FullName.Should().Be("New Name");
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ValidId_SoftDeletesAndSaves()
    {
        var existing = MakeStudent();
        _studentRepo.Setup(r => r.GetByIdAsync(StudentId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(StudentId, "admin@test.com");

        result.Success.Should().BeTrue();
        _studentRepo.Verify(r => r.Delete(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFailure()
    {
        _studentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Student?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _studentRepo.Verify(r => r.Delete(It.IsAny<Student>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_SetsUpdatedByBeforeSave()
    {
        var existing = MakeStudent();
        _studentRepo.Setup(r => r.GetByIdAsync(StudentId)).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteAsync(StudentId, "admin@test.com");

        existing.UpdatedBy.Should().Be("admin@test.com");
        existing.UpdatedAt.Should().NotBeNull();
    }
}
