using AutoMapper;
using HHMCore.Core.DTOs.Semester;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public class SemesterMappingProfile : Profile
{
    public SemesterMappingProfile()
    {
        CreateMap<Semester, SemesterResponseDto>();
    }
}