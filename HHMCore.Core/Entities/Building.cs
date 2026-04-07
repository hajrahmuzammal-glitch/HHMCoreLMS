namespace HHMCore.Core.Entities
{
    public class Building : BaseEntity
    {
        public string Name { get; set; } = string.Empty;   
        public string? Code { get; set; }                   
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}