using HHMCore.Core.Enums;

namespace HHMCore.Core.Entities;

public class Room : BaseEntity
{
    public string RoomNumber { get; set; } = string.Empty;  
    public string Building { get; set; } = string.Empty;    
    public int Capacity { get; set; }                        
    public RoomType RoomType { get; set; }                  
    public bool IsActive { get; set; } = true;

  
    public ICollection<CourseAssignment> CourseAssignments { get; set; }
        = new List<CourseAssignment>();
}