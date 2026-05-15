namespace HHMCore.Core.Entities;

public class Semester : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = false;

    public ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<FeeRecord> FeeRecords { get; set; } = new List<FeeRecord>();

}
