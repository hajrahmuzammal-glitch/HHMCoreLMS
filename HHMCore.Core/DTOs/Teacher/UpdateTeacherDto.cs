using HHMCore.Core.Enums;


namespace HHMCore.Core.DTOs.Teacher;

public class UpdateTeacherDto
{
    public string? FullName { get; set; }
    public Guid? DesignationId { get; set; }
    public Gender? Gender { get; set; }
    public decimal? Salary { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Qualification { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? JoiningDate { get; set; }
}
