namespace HHMCore.Core.DTOs.Semester;

public class UpdateSemesterDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}