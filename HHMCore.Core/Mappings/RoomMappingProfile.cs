using AutoMapper;
using HHMCore.Core.DTOs.Room;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public class RoomMappingProfile : Profile
{
    public RoomMappingProfile()
    {
        CreateMap<Room, RoomResponseDto>()
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType.ToString()));
    }
}