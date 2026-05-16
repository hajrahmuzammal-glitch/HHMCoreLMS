using HHMCore.Core.Enums;

namespace HHMCore.Core.DTOs.Teacher;

public class CreateTeacherDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Cnic { get; set; } = string.Empty;
    public Guid DesignationId { get; set; }
    public Gender Gender { get; set; }
    public decimal Salary { get; set; }
    public Guid DepartmentId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public DateOnly JoiningDate { get; set; }
}
