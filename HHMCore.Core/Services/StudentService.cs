using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Student;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HHMCore.Core.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ApiResponse<StudentResponseDto>> CreateAsync(CreateStudentDto dto, string createdBy)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return ApiResponse<StudentResponseDto>.Fail("A user with this email already exists.");

            var existingRollNumber = await _unitOfWork.Students
                .FindAsync(s => s.RollNumber == dto.RollNumber);
            if (existingRollNumber.Any())
                return ApiResponse<StudentResponseDto>.Fail("A student with this roll number already exists.");

            var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
            if (department == null)
                return ApiResponse<StudentResponseDto>.Fail("Department not found.");

            var appUser = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };

            var userResult = await _userManager.CreateAsync(appUser, dto.Password);
            if (!userResult.Succeeded)
            {
                var errors = userResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse<StudentResponseDto>.Fail(string.Join(", ", errors));
            }

            try
            {
                await _userManager.AddToRoleAsync(appUser, "Student");

                var student = new Student
                {
                    UserId = appUser.Id,
                    RollNumber = dto.RollNumber,
                    DepartmentId = dto.DepartmentId,
                    CurrentSemesterNumber = dto.CurrentSemesterNumber,
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth,
                    Status = "Active",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Students.AddAsync(student);
                await _unitOfWork.SaveChangesAsync();

                var createdStudent = await _unitOfWork.Students
                    .GetByIdWithIncludesAsync(student.Id, s => s.User, s => s.Department);

                var response = _mapper.Map<StudentResponseDto>(createdStudent);
                return ApiResponse<StudentResponseDto>.Ok(response, "Student created successfully.");
            }
            catch
            {
                await _userManager.DeleteAsync(appUser);
                return ApiResponse<StudentResponseDto>.Fail("Failed to create student profile. Please try again.");
            }
        }

        public async Task<ApiResponse<StudentResponseDto>> GetByIdAsync(Guid id)
        {
            var student = await _unitOfWork.Students
                .GetByIdWithIncludesAsync(id, s => s.User, s => s.Department);

            if (student == null)
                return ApiResponse<StudentResponseDto>.Fail("Student not found.");

            var response = _mapper.Map<StudentResponseDto>(student);
            return ApiResponse<StudentResponseDto>.Ok(response, "Student retrieved successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<StudentResponseDto>>> GetAllAsync()
        {
            var students = await _unitOfWork.Students
                .GetAllWithIncludesAsync(s => s.User, s => s.Department);

            var response = _mapper.Map<IReadOnlyList<StudentResponseDto>>(students);
            return ApiResponse<IReadOnlyList<StudentResponseDto>>.Ok(response, "Students retrieved successfully.");
        }

        public async Task<ApiResponse<StudentResponseDto>> UpdateAsync(UpdateStudentDto dto, string updatedBy)
        {
            var student = await _unitOfWork.Students
                .GetByIdWithIncludesAsync(dto.Id, s => s.User, s => s.Department);

            if (student == null)
                return ApiResponse<StudentResponseDto>.Fail("Student not found.");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                var appUser = await _userManager.FindByIdAsync(student.UserId);
                if (appUser != null)
                {
                    appUser.FullName = dto.FullName;
                    await _userManager.UpdateAsync(appUser);
                }
            }

            if (dto.DepartmentId.HasValue)
            {
                var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId.Value);
                if (department == null)
                    return ApiResponse<StudentResponseDto>.Fail("Department not found.");
                student.DepartmentId = dto.DepartmentId.Value;
            }

            student.CurrentSemesterNumber = dto.CurrentSemesterNumber ?? student.CurrentSemesterNumber;
            student.Address = string.IsNullOrWhiteSpace(dto.Address) ? student.Address : dto.Address;
            student.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? student.PhoneNumber : dto.PhoneNumber;
            student.DateOfBirth = dto.DateOfBirth ?? student.DateOfBirth;
            student.UpdatedBy = updatedBy;
            student.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Students
                .GetByIdWithIncludesAsync(dto.Id, s => s.User, s => s.Department);

            var response = _mapper.Map<StudentResponseDto>(updated);
            return ApiResponse<StudentResponseDto>.Ok(response, "Student updated successfully."); 
        }
        public async Task<ApiResponse> DeleteAsync(Guid id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);

            if (student == null)
                return ApiResponse.Fail("Student not found.");

            _unitOfWork.Students.Delete(student);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Student deleted successfully.");
        }

        public async Task<ApiResponse<StudentResponseDto>> GetMeAsync(string userId)
        {
            var student = await _unitOfWork.Students
                .FindOneWithIncludesAsync(s => s.UserId == userId, s => s.User, s => s.Department);

            if (student is null)
                return ApiResponse<StudentResponseDto>.Fail("Student profile not found.");

            var response = _mapper.Map<StudentResponseDto>(student);
            return ApiResponse<StudentResponseDto>.Ok(response, "Profile retrieved successfully.");
        }
    }
}