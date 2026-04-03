
using AutoMapper;
using HHMCore.Core.DTOs.CourseAssignment;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public class CourseAssignmentMappingProfile : Profile
{
    public CourseAssignmentMappingProfile()
    {
        CreateMap<CourseAssignment, CourseAssignmentResponseDto>()
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src =>
                    src.Teacher != null ? src.Teacher.User.FullName : string.Empty))
            .ForMember(dest => dest.EmployeeId,
                opt => opt.MapFrom(src =>
                    src.Teacher != null ? src.Teacher.EmployeeId : string.Empty))
            .ForMember(dest => dest.CourseName,
                opt => opt.MapFrom(src =>
                    src.Course != null ? src.Course.Name : string.Empty))
            .ForMember(dest => dest.CourseCode,
                opt => opt.MapFrom(src =>
                    src.Course != null ? src.Course.Code : string.Empty))
            .ForMember(dest => dest.SemesterName,
                opt => opt.MapFrom(src =>
                    src.Semester != null ? src.Semester.Name : string.Empty))
            .ForMember(dest => dest.RoomNumber,
                opt => opt.MapFrom(src =>
                    src.Room != null ? src.Room.RoomNumber : string.Empty))
            .ForMember(dest => dest.Building,
                opt => opt.MapFrom(src => 
                    src.Room != null ? src.Room.Building : string.Empty))
            .ForMember(dest => dest.TimeSlotLabel,
                opt => opt.MapFrom(src =>
                    src.TimeSlot != null ? src.TimeSlot.Label : string.Empty));

    }
}