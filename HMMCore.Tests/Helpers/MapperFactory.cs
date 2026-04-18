using AutoMapper;
using HHMCore.Core.Mappings;

namespace HHMCore.Tests.Helpers;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuthMappingProfile>();
            cfg.AddProfile<DepartmentMappingProfile>();
            cfg.AddProfile<CourseMappingProfile>();
            cfg.AddProfile<StudentMappingProfile>();
            cfg.AddProfile<TeacherMappingProfile>();
            cfg.AddProfile<DesignationMappingProfile>();
            cfg.AddProfile<SemesterMappingProfile>();
            cfg.AddProfile<RoomMappingProfile>();
            cfg.AddProfile<TimeSlotMappingProfile>();
            cfg.AddProfile<CourseAssignmentMappingProfile>();
            cfg.AddProfile<BuildingMappingProfile>();
        });

        return config.CreateMapper();
    }
}