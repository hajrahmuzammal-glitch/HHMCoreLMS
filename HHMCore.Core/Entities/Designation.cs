namespace HHMCore.Core.Entities;

public class Designation : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}