using AutoMapper;
using HHMCore.Core.DTOs.Building;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings
{
    public class BuildingMappingProfile : Profile
    {
        public BuildingMappingProfile()
        {
            CreateMap<Building, BuildingResponseDto>()
                .ForMember(dest => dest.RoomCount,
                    opt => opt.MapFrom(src => src.Rooms.Count(r => !r.IsDeleted)));
        }
    }
}