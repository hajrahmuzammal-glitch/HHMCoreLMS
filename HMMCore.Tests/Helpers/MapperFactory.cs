using AutoMapper;
using HHMCore.Core.Mappings;

namespace HHMCore.Tests.Helpers;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RoomMappingProfile>();
        });

        return config.CreateMapper();
    }
}