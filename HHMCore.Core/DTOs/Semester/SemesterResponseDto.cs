

namespace HHMCore.Core.DTOs.Semester;

public class SemesterResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int SemesterNumber { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
