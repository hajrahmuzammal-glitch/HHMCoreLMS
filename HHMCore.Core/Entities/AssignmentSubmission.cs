namespace HHMCore.Core.Entities;

public class AssignmentSubmission : BaseEntity
{
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public string? FilePath { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsLate { get; set; } = false;

    public decimal? MarksObtained { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }

}
