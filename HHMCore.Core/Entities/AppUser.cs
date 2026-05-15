using Microsoft.AspNetCore.Identity;

namespace HHMCore.Core.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
