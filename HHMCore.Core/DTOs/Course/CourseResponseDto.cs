namespace HHMCore.Core.DTOs.Course;

public class CourseResponseDto
{
    //Course Info
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    //Academic Infor
    public int CreditHours { get; set; }
    public int SemesterNumber { get; set; }
    public bool IsActive { get; set; }

    //Department
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    //Audit Info
    public DateTimeOffset CreatedAt { get; set; }
}
