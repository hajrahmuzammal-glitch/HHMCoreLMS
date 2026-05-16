
namespace HHMCore.Core.DTOs.Department;

public class DepartmentResponseDto
{
    //Department Info
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }

    //Status Info
    public bool IsActive { get; init; }

    //Audit Info
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public string? UpdatedBy { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    //dont think need more changes as i di not add or delete anything except changing
    //datetime to datetimeoffset , so required to handle the service ,validators
}
