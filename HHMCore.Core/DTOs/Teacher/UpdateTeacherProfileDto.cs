using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.DTOs.Teacher;

public class UpdateTeacherProfileDto
{
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Qualification { get; set; } = string.Empty;
}