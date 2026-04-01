

namespace HHMCore.Core.DTOs.Semester;

public class CreateSemesterDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
