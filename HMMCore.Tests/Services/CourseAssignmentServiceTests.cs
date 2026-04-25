using AutoMapper;
using FluentAssertions;
using Moq;
using HHMCore.Core.DTOs.CourseAssignment;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Services;
using HHMCore.Tests.Helpers;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace HHMCore.Tests.Services;

public class CourseAssignmentServiceTests
{
    // ── Mocks ─────────────────────────────────────────────────────────────────
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IGenericRepository<CourseAssignment>> _caRepo;
    private readonly Mock<IGenericRepository<Teacher>> _teacherRepo;
    private readonly Mock<IGenericRepository<Course>> _courseRepo;
    private readonly Mock<IGenericRepository<Semester>> _semesterRepo;
    private readonly Mock<IGenericRepository<Room>> _roomRepo;
    private readonly Mock<IGenericRepository<TimeSlot>> _timeSlotRepo;
    private readonly IMapper _mapper;
    private readonly CourseAssignmentService _sut;

    // ── Fixed IDs — deterministic, never use Guid.NewGuid() in tests ──────────
    private static readonly Guid TeacherId = Guid.Parse("aaaa0000-0000-0000-0000-000000000001");
    private static readonly Guid CourseId = Guid.Parse("bbbb0000-0000-0000-0000-000000000002");
    private static readonly Guid SemesterId = Guid.Parse("cccc0000-0000-0000-0000-000000000003");
    private static readonly Guid RoomId = Guid.Parse("dddd0000-0000-0000-0000-000000000004");
    private static readonly Guid TimeSlotId = Guid.Parse("eeee0000-0000-0000-0000-000000000005");
    private static readonly Guid DepartmentId = Guid.Parse("ffff0000-0000-0000-0000-000000000006");
    private static readonly Guid AssignId = Guid.Parse("aaab0000-0000-0000-0000-000000000007");

    public CourseAssignmentServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _caRepo = new Mock<IGenericRepository<CourseAssignment>>();
        _teacherRepo = new Mock<IGenericRepository<Teacher>>();
        _courseRepo = new Mock<IGenericRepository<Course>>();
        _semesterRepo = new Mock<IGenericRepository<Semester>>();
        _roomRepo = new Mock<IGenericRepository<Room>>();
        _timeSlotRepo = new Mock<IGenericRepository<TimeSlot>>();
        _mapper = MapperFactory.Create();

        _uow.Setup(u => u.CourseAssignments).Returns(_caRepo.Object);
        _uow.Setup(u => u.Teachers).Returns(_teacherRepo.Object);
        _uow.Setup(u => u.Courses).Returns(_courseRepo.Object);
        _uow.Setup(u => u.Semesters).Returns(_semesterRepo.Object);
        _uow.Setup(u => u.Rooms).Returns(_roomRepo.Object);
        _uow.Setup(u => u.TimeSlots).Returns(_timeSlotRepo.Object);

        _sut = new CourseAssignmentService(_uow.Object, _mapper);
    }

    // ── Shared test data ──────────────────────────────────────────────────────

    private static CreateCourseAssignmentDto ValidDto() => new()
    {
        TeacherId = TeacherId,
        CourseId = CourseId,
        SemesterId = SemesterId,
        RoomId = RoomId,
        TimeSlotId = TimeSlotId,
        Section = "A",
        MaxEnrollment = 30
    };

    private static Teacher ValidTeacher() => new()
    {
        Id = TeacherId,
        EmployeeId = "EMP-001",
        IsDeleted = false,
        User = new AppUser { FullName = "Dr. Ahmed" }
    };

    private static Course ValidCourse() => new()
    {
        Id = CourseId,
        Name = "Data Structures",
        Code = "CS-301",
        CreditHours = 3,
        DepartmentId = DepartmentId,
        IsDeleted = false
    };

    private static Semester ValidSemester() => new()
    {
        Id = SemesterId,
        Name = "Fall 2025",
        IsDeleted = false
    };

    private static Room ValidRoom() => new()
    {
        Id = RoomId,
        RoomNumber = "A101",
        Capacity = 50,           // higher than MaxEnrollment=30 — no conflict
        IsDeleted = false,
        Building = new Building { Name = "Main Block" }
    };

    private static CourseAssignment FullAssignment() => new()
    {
        Id = AssignId,
        TeacherId = TeacherId,
        CourseId = CourseId,
        SemesterId = SemesterId,
        RoomId = RoomId,
        TimeSlotId = TimeSlotId,
        DepartmentId = DepartmentId,
        Section = "A",
        MaxEnrollment = 30,
        Course = ValidCourse(),
        Department = new Department { Name = "Computer Science" },
        Teacher = ValidTeacher(),
        Room = ValidRoom(),
        TimeSlot = new TimeSlot
        {
            Label = "Mon/Wed 09:00–11:00",
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(11, 0)
        },
        Semester = ValidSemester()
    };

    // ── Setup helpers ─────────────────────────────────────────────────────────

    private void AllEntitiesExist()
    {
        _teacherRepo.Setup(r => r.GetByIdAsync(TeacherId)).ReturnsAsync(ValidTeacher());
        _courseRepo.Setup(r => r.GetByIdAsync(CourseId)).ReturnsAsync(ValidCourse());
        _semesterRepo.Setup(r => r.GetByIdAsync(SemesterId)).ReturnsAsync(ValidSemester());
        _roomRepo.Setup(r => r.GetByIdAsync(RoomId)).ReturnsAsync(ValidRoom());
        _timeSlotRepo.Setup(r => r.ExistsAsync(
            It.IsAny<Expression<Func<TimeSlot, bool>>>())).ReturnsAsync(true);
    }

    private void NoConflicts()
    {
        _caRepo.Setup(r => r.ExistsAsync(
            It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
            .ReturnsAsync(false);
    }

    private void SetupSaveAndRefetch()
    {
        _caRepo.Setup(r => r.AddAsync(It.IsAny<CourseAssignment>()))
               .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _caRepo.Setup(r => r.GetByIdWithDetailsAsync(
                   It.IsAny<Guid>(),
                   It.IsAny<Func<IQueryable<CourseAssignment>,
                                  IQueryable<CourseAssignment>>>()))
               .ReturnsAsync(FullAssignment());
    }

    // ── HAPPY PATH ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        AllEntitiesExist();
        NoConflicts();
        SetupSaveAndRefetch();

        var result = await _sut.CreateAsync(ValidDto(), "admin@test.com");

        result.Success.Should().BeTrue();
        result.Data!.CourseName.Should().Be("Data Structures");
        result.Data.TeacherName.Should().Be("Dr. Ahmed");
        result.Data.BuildingName.Should().Be("Main Block");
        result.Data.SemesterName.Should().Be("Fall 2025");
        result.Data.Section.Should().Be("A");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesExactlyOnce()
    {
        AllEntitiesExist();
        NoConflicts();
        SetupSaveAndRefetch();

        await _sut.CreateAsync(ValidDto(), "admin@test.com");

        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _caRepo.Verify(r => r.AddAsync(It.IsAny<CourseAssignment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DepartmentIdDerivedFromCourse_NotFromDto()
    {
        // Proves DepartmentId is taken from Course, not manually supplied
        AllEntitiesExist();
        NoConflicts();

        CourseAssignment? captured = null;
        _caRepo.Setup(r => r.AddAsync(It.IsAny<CourseAssignment>()))
               .Callback<CourseAssignment>(ca => captured = ca)
               .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _caRepo.Setup(r => r.GetByIdWithDetailsAsync(
                   It.IsAny<Guid>(),
                   It.IsAny<Func<IQueryable<CourseAssignment>,
                                  IQueryable<CourseAssignment>>>()))
               .ReturnsAsync(FullAssignment());

        await _sut.CreateAsync(ValidDto(), "admin@test.com");

        captured.Should().NotBeNull();
        captured!.DepartmentId.Should().Be(DepartmentId); // came from Course.DepartmentId
    }

    // ── FK VALIDATION ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_TeacherNotFound_ReturnsFail()
    {
        _teacherRepo.Setup(r => r.GetByIdAsync(TeacherId))
                    .ReturnsAsync((Teacher?)null);

        var result = await _sut.CreateAsync(ValidDto(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Teacher");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CourseNotFound_ReturnsFail()
    {
        _teacherRepo.Setup(r => r.GetByIdAsync(TeacherId)).ReturnsAsync(ValidTeacher());
        _courseRepo.Setup(r => r.GetByIdAsync(CourseId)).ReturnsAsync((Course?)null);

        var result = await _sut.CreateAsync(ValidDto(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Course");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RoomNotFound_ReturnsFail()
    {
        _teacherRepo.Setup(r => r.GetByIdAsync(TeacherId)).ReturnsAsync(ValidTeacher());
        _courseRepo.Setup(r => r.GetByIdAsync(CourseId)).ReturnsAsync(ValidCourse());
        _semesterRepo.Setup(r => r.GetByIdAsync(SemesterId)).ReturnsAsync(ValidSemester());
        _roomRepo.Setup(r => r.GetByIdAsync(RoomId)).ReturnsAsync((Room?)null);

        var result = await _sut.CreateAsync(ValidDto(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Room");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ── CAPACITY VALIDATION ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_MaxEnrollmentExceedsRoomCapacity_ReturnsFail()
    {
        var smallRoom = new Room
        {
            Id = RoomId,
            RoomNumber = "Tiny-01",
            Capacity = 10,         // room only fits 10
            IsDeleted = false
        };

        _teacherRepo.Setup(r => r.GetByIdAsync(TeacherId)).ReturnsAsync(ValidTeacher());
        _courseRepo.Setup(r => r.GetByIdAsync(CourseId)).ReturnsAsync(ValidCourse());
        _semesterRepo.Setup(r => r.GetByIdAsync(SemesterId)).ReturnsAsync(ValidSemester());
        _roomRepo.Setup(r => r.GetByIdAsync(RoomId)).ReturnsAsync(smallRoom);
        _timeSlotRepo.Setup(r => r.ExistsAsync(
            It.IsAny<Expression<Func<TimeSlot, bool>>>())).ReturnsAsync(true);

        var dto = ValidDto();
        dto.MaxEnrollment = 30;     // 30 > room capacity of 10

        var result = await _sut.CreateAsync(dto, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("capacity");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ── CONFLICT RULES ────────────────────────────────────────────────────────
    // SetupSequence order matches service method order:
    // Call 1=room, Call 2=teacher, Call 3=course, Call 4=section

    [Fact]
    public async Task CreateAsync_RoomConflict_ReturnsFail()
    {
        AllEntitiesExist();
        _caRepo.SetupSequence(r => r.ExistsAsync(
                   It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
               .ReturnsAsync(true);     // call 1 — room conflict

        var result = await _sut.CreateAsync(ValidDto(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("room");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_TeacherConflict_ReturnsFail()
    {
        AllEntitiesExist();
        _caRepo.SetupSequence(r => r.ExistsAsync(
                   It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
               .ReturnsAsync(false)     // room: clear
               .ReturnsAsync(true);     // teacher: conflict

        var result = await _sut.CreateAsync(ValidDto(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("teacher");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CourseConflict_ReturnsFail()
    {
        AllEntitiesExist();
        _caRepo.SetupSequence(r => r.ExistsAsync(
                   It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
               .ReturnsAsync(false)     // room: clear
               .ReturnsAsync(false)     // teacher: clear
               .ReturnsAsync(true);     // course: conflict

        var result = await _sut.CreateAsync(ValidDto(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("course");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SectionConflict_ReturnsFail()
    {
        AllEntitiesExist();
        _caRepo.SetupSequence(r => r.ExistsAsync(
                   It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
               .ReturnsAsync(false)     // room: clear
               .ReturnsAsync(false)     // teacher: clear
               .ReturnsAsync(false)     // course: clear
               .ReturnsAsync(true);     // section: conflict

        var result = await _sut.CreateAsync(ValidDto(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("section");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ── UPDATE TESTS ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_AssignmentNotFound_ReturnsFail()
    {
        _caRepo.Setup(r => r.GetByIdAsync(AssignId))
               .ReturnsAsync((CourseAssignment?)null);

        var result = await _sut.UpdateAsync(
            AssignId, new UpdateCourseAssignmentDto(), "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OnlyTeacherChanged_RunsTeacherConflictOnly()
    {
        // Arrange — only TeacherId is changing
        var existing = new CourseAssignment
        {
            Id = AssignId,
            TeacherId = TeacherId,
            RoomId = RoomId,
            TimeSlotId = TimeSlotId,
            SemesterId = SemesterId,
            DepartmentId = DepartmentId,
            Section = "A",
            MaxEnrollment = 30,
            IsDeleted = false
        };

        var newTeacherId = Guid.Parse("1111aaaa-0000-0000-0000-000000000099");

        _caRepo.Setup(r => r.GetByIdAsync(AssignId)).ReturnsAsync(existing);
        _teacherRepo.Setup(r => r.ExistsAsync(
            It.IsAny<Expression<Func<Teacher, bool>>>())).ReturnsAsync(true);

        // No teacher conflict
        _caRepo.Setup(r => r.ExistsAsync(
            It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
            .ReturnsAsync(false);

        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _caRepo.Setup(r => r.GetByIdWithDetailsAsync(
                   It.IsAny<Guid>(),
                   It.IsAny<Func<IQueryable<CourseAssignment>,
                                  IQueryable<CourseAssignment>>>()))
               .ReturnsAsync(FullAssignment());

        var dto = new UpdateCourseAssignmentDto { TeacherId = newTeacherId };

        // Act
        var result = await _sut.UpdateAsync(AssignId, dto, "admin@test.com");

        // Assert
        result.Success.Should().BeTrue();
        // Room conflict check should NOT have run — RoomId did not change
        // Only 1 ExistsAsync call should have happened (teacher conflict only)
        _caRepo.Verify(r => r.ExistsAsync(
            It.IsAny<Expression<Func<CourseAssignment, bool>>>()),
            Times.Once);                // teacher conflict only — not room
    }

    [Fact]
    public async Task UpdateAsync_MaxEnrollmentExceedsRoomCapacity_ReturnsFail()
    {
        var existing = new CourseAssignment
        {
            Id = AssignId,
            TeacherId = TeacherId,
            RoomId = RoomId,
            TimeSlotId = TimeSlotId,
            SemesterId = SemesterId,
            DepartmentId = DepartmentId,
            Section = "A",
            MaxEnrollment = 30,
            IsDeleted = false
        };

        var smallRoom = new Room { Id = RoomId, Capacity = 10, IsDeleted = false };

        _caRepo.Setup(r => r.GetByIdAsync(AssignId)).ReturnsAsync(existing);
        _roomRepo.Setup(r => r.GetByIdAsync(RoomId)).ReturnsAsync(smallRoom);

        var dto = new UpdateCourseAssignmentDto { MaxEnrollment = 50 }; // 50 > 10

        var result = await _sut.UpdateAsync(AssignId, dto, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("capacity");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);

    }
    [Fact]
    public async Task UpdateAsync_ValidChange_SavesOnce()
    {
        var existing = FullAssignment();

        _caRepo.Setup(r => r.GetByIdAsync(AssignId)).ReturnsAsync(existing);
        _caRepo.Setup(r => r.ExistsAsync(
            It.IsAny<Expression<Func<CourseAssignment, bool>>>()))
            .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _caRepo.Setup(r => r.GetByIdWithDetailsAsync(
                   It.IsAny<Guid>(),
                   It.IsAny<Func<IQueryable<CourseAssignment>,
                                  IQueryable<CourseAssignment>>>()))
               .ReturnsAsync(FullAssignment());

        var result = await _sut.UpdateAsync(
            AssignId,
            new UpdateCourseAssignmentDto { Section = "B" },
            "admin@test.com");

        result.Success.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsSuccess()
    {
        _caRepo
            .Setup(r => r.GetByIdWithDetailsAsync(
                AssignId,
                It.IsAny<Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>>>()))
            .ReturnsAsync(FullAssignment());

        var result = await _sut.GetByIdAsync(AssignId);

        result.Success.Should().BeTrue();
        result.Data!.CourseName.Should().Be("Data Structures");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFail()
    {
        _caRepo
            .Setup(r => r.GetByIdWithDetailsAsync(
                AssignId,
                It.IsAny<Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>>>()))
            .ReturnsAsync((CourseAssignment?)null);

        var result = await _sut.GetByIdAsync(AssignId);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // ── GET ALL ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllAssignments()
    {
        _caRepo
            .Setup(r => r.GetAllWithDetailsAsync(
                It.IsAny<Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>>>()))
            .ReturnsAsync(new List<CourseAssignment> { FullAssignment() });

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data!.Count.Should().Be(1);
        result.Data.First().CourseName.Should().Be("Data Structures");
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsSuccessWithEmptyList()
    {
        _caRepo
            .Setup(r => r.GetAllWithDetailsAsync(
                It.IsAny<Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>>>()))
            .ReturnsAsync(new List<CourseAssignment>());

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data!.Count.Should().Be(0);
    }

    // ── GET BY SEMESTER ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySemesterAsync_ReturnsMatchingAssignments()
    {
        _caRepo
            .Setup(r => r.FindWithDetailsAsync(
                It.IsAny<Expression<Func<CourseAssignment, bool>>>(),
                It.IsAny<Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>>>()))
            .ReturnsAsync(new List<CourseAssignment> { FullAssignment() });

        var result = await _sut.GetBySemesterAsync(SemesterId);

        result.Success.Should().BeTrue();
        result.Data!.Count.Should().Be(1);
        result.Data.First().SemesterName.Should().Be("Fall 2025");
    }

    [Fact]
    public async Task GetBySemesterAsync_NoAssignments_ReturnsEmptyList()
    {
        _caRepo
            .Setup(r => r.FindWithDetailsAsync(
                It.IsAny<Expression<Func<CourseAssignment, bool>>>(),
                It.IsAny<Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>>>()))
            .ReturnsAsync(new List<CourseAssignment>());

        var result = await _sut.GetBySemesterAsync(SemesterId);

        result.Success.Should().BeTrue();
        result.Data!.Count.Should().Be(0);
    }

    // ── GET BY TEACHER ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByTeacherAsync_TeacherNotFound_ReturnsFail()
    {
        _teacherRepo
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Teacher, bool>>>()))
            .ReturnsAsync(false);

        var result = await _sut.GetByTeacherAsync(TeacherId);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Teacher");
    }

    [Fact]
    public async Task GetByTeacherAsync_Found_ReturnsAssignments()
    {
        _teacherRepo
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Teacher, bool>>>()))
            .ReturnsAsync(true);

        _caRepo
            .Setup(r => r.FindWithDetailsAsync(
                It.IsAny<Expression<Func<CourseAssignment, bool>>>(),
                It.IsAny<Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>>>()))
            .ReturnsAsync(new List<CourseAssignment> { FullAssignment() });

        var result = await _sut.GetByTeacherAsync(TeacherId);

        result.Success.Should().BeTrue();
        result.Data!.Count.Should().Be(1);
    }

    // ── GET MY ASSIGNMENTS ────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyAssignmentsAsync_TeacherProfileNotFound_ReturnsFail()
    {
        _teacherRepo
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Teacher, bool>>>()))
            .ReturnsAsync((Teacher?)null);

        var result = await _sut.GetMyAssignmentsAsync("user-123");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetMyAssignmentsAsync_Found_ReturnsAssignments()
    {
        _teacherRepo
            .Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Teacher, bool>>>()))
            .ReturnsAsync(ValidTeacher());

        _caRepo
            .Setup(r => r.FindWithDetailsAsync(
                It.IsAny<Expression<Func<CourseAssignment, bool>>>(),
                It.IsAny<Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>>>()))
            .ReturnsAsync(new List<CourseAssignment> { FullAssignment() });

        var result = await _sut.GetMyAssignmentsAsync("user-123");

        result.Success.Should().BeTrue();
        result.Data!.Count.Should().Be(1);
    }
    // ── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFail()
    {
        _caRepo
            .Setup(r => r.GetByIdAsync(AssignId))
            .ReturnsAsync((CourseAssignment?)null);

        var result = await _sut.DeleteAsync(AssignId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_HasAttendance_ReturnsFail()
    {
        _caRepo
            .Setup(r => r.GetByIdAsync(AssignId))
            .ReturnsAsync(FullAssignment());

        _uow.Setup(u => u.Attendances.ExistsAsync(
                It.IsAny<Expression<Func<Attendance, bool>>>()))
            .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(AssignId, "admin@test.com");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("attendance");
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Valid_SoftDeletesAndSaves()
    {
        var assignment = FullAssignment();

        _caRepo
            .Setup(r => r.GetByIdAsync(AssignId))
            .ReturnsAsync(assignment);

        _uow.Setup(u => u.Attendances.ExistsAsync(
                It.IsAny<Expression<Func<Attendance, bool>>>()))
            .ReturnsAsync(false);

        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(AssignId, "admin@test.com");

        result.Success.Should().BeTrue();
        assignment.IsDeleted.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _caRepo.Verify(r => r.Update(assignment), Times.Once);
    }
}
