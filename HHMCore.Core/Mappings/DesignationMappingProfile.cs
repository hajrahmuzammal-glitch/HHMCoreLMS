using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HHMCore.Core.DTOs.Designation;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public class DesignationMappingProfile : Profile
{
    public DesignationMappingProfile()
    {
        CreateMap<Designation, DesignationResponseDto>();
        CreateMap<CreateDesignationDto, Designation>();
    }
}
