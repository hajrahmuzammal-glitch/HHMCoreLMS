using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.DTOs.Department;

public class UpdateDepartmentDto
{
    public string? Name { get; set; } 
    public string?Code { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}