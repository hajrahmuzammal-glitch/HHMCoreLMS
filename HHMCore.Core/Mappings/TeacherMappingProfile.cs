using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HHMCore.Core.DTOs.Teacher;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings
{
    public class TeacherMappingProfile : Profile
    {
        public TeacherMappingProfile()
        {
            CreateMap<Teacher, TeacherResponseDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.DepartmentName,
                    opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty));
        }
    }
}