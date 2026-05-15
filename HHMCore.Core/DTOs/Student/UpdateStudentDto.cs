namespace HHMCore.Core.DTOs.Student;

public class UpdateStudentDto
{
    public string? FullName { get; set; } 
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? CurrentSemesterNumber { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Status { get; set; }
}