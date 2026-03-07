using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public class DepartmentMappingProfile : Profile
{
    public DepartmentMappingProfile()
    {
        // Map Department entity → DepartmentResponseDto
        // AutoMapper matches properties by name automatically
        // Id → Id, Name → Name, Code → Code etc.
        CreateMap<Department, DepartmentResponseDto>();
    }
}