
namespace HHMCore.Core.DTOs.Teacher;

public class TeacherResponseDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string EmployeeId { get; init; } = string.Empty;
    public string Cnic { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string DesignationTitle { get; init; } = string.Empty;
    public Guid DesignationId { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string Qualification { get; init; } = string.Empty;
    public DateOnly JoiningDate { get; init; }
    public decimal Salary { get; init; }
    public string Status { get; init; } = string.Empty;

    public Guid DepartmentId { get; init; }
    public string DepartmentName { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    //then we need to see who has deeted these filed too? althoug i am using soft delete
    //and i have the deletedBy in the parameter of the delete function but it should be shown to admin somewhere?

}
