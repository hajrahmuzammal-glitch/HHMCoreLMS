namespace HHMCore.Core.DTOs.Building
{
    public class CreateBuildingDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}