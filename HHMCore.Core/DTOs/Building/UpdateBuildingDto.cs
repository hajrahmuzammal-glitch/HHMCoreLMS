namespace HHMCore.Core.DTOs.Building
{
    
    public class UpdateBuildingDto
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}