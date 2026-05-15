
namespace HHMCore.Core.DTOs.Department;

public class UpdateDepartmentDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }

    // we dont have updatedAt and updatedBy here as we are initializing them in service , right?
    //so there weill be no problem with  the validators
}
