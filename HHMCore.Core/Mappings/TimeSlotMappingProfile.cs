using AutoMapper;
using HHMCore.Core.DTOs.TimeSlot;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public class TimeSlotMappingProfile : Profile
{
    public TimeSlotMappingProfile()
    {
        CreateMap<TimeSlot, TimeSlotResponseDto>()
            .ForMember(dest => dest.DaysValue, opt => opt.MapFrom(src => (int)src.Days))
            .ForMember(dest => dest.DaysDisplay, opt => opt.MapFrom(src => BuildDaysDisplay(src.Days)))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString("HH:mm")))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString("HH:mm")));
    }

    private static string BuildDaysDisplay(HHMCore.Core.Enums.LmsDaysOfWeek days)
    {
        var map = new Dictionary<HHMCore.Core.Enums.LmsDaysOfWeek, string>
        {
            { Enums.LmsDaysOfWeek.Monday,    "Mon" },
            { Enums.LmsDaysOfWeek.Tuesday,   "Tue" },
            { Enums.LmsDaysOfWeek.Wednesday, "Wed" },
            { Enums.LmsDaysOfWeek.Thursday,  "Thu" },
            { Enums.LmsDaysOfWeek.Friday,    "Fri" },
            { Enums.LmsDaysOfWeek.Saturday,  "Sat" }
        };

        return string.Join("/", map.Where(kvp => days.HasFlag(kvp.Key)).Select(kvp => kvp.Value));
    }
}