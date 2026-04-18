// HHMCore.Core/Mappings/CourseAssignmentMappingProfile.cs

using AutoMapper;
using HHMCore.Core.DTOs.CourseAssignment;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public class CourseAssignmentMappingProfile : Profile
{
    public CourseAssignmentMappingProfile()
    {
        CreateMap<CourseAssignment, CourseAssignmentResponseDto>()
            // Course
            .ForMember(d => d.CourseName,
                o => o.MapFrom(s => s.Course.Name))
            .ForMember(d => d.CourseCode,
                o => o.MapFrom(s => s.Course.Code))
            .ForMember(d => d.CreditHours,
                o => o.MapFrom(s => s.Course.CreditHours))

            // Department — comes from Course.Department
            .ForMember(d => d.DepartmentName,
                o => o.MapFrom(s => s.Department.Name))

            // Teacher — nested: Teacher.User.FullName
            .ForMember(d => d.TeacherName,
                o => o.MapFrom(s => s.Teacher.User.FullName))
            .ForMember(d => d.EmployeeId,
                o => o.MapFrom(s => s.Teacher.EmployeeId))

            // Room — nested: Room.Building.Name
            .ForMember(d => d.RoomNumber,
                o => o.MapFrom(s => s.Room.RoomNumber))
            .ForMember(d => d.BuildingName,
                o => o.MapFrom(s => s.Room.Building.Name))
            .ForMember(d => d.RoomCapacity,
                o => o.MapFrom(s => s.Room.Capacity))

            // TimeSlot
            .ForMember(d => d.TimeSlotLabel,
                o => o.MapFrom(s => s.TimeSlot.Label))
            .ForMember(d => d.Days,
                o => o.MapFrom(s => s.TimeSlot.Days.ToString()))
            .ForMember(d => d.StartTime,
                o => o.MapFrom(s => s.TimeSlot.StartTime.ToString("HH:mm")))
            .ForMember(d => d.EndTime,
                o => o.MapFrom(s => s.TimeSlot.EndTime.ToString("HH:mm")))

            // Semester
            .ForMember(d => d.SemesterName,
                o => o.MapFrom(s => s.Semester.Name));
    }
}