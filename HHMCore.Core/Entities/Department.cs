namespace HHMCore.Core.Entities;

public class Department : BaseEntity
{
    //Depatment Info
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    //Navigation Properties
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

}
