using Microsoft.AspNetCore.Identity;
using Manabi.Shared.Models;

namespace Manabi.Api.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Student;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public TeacherProfile? TeacherProfile { get; set; }
}
