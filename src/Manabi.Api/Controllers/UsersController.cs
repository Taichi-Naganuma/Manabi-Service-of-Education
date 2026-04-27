using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;
using Manabi.Shared.Models.Responses;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserManager<AppUser> userManager, AppDbContext db) : ControllerBase
{
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfileResponse>> GetUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var teacherProfile = await db.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == userId);

        return Ok(new UserProfileResponse
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            IsTeacher = teacherProfile is not null,
            Skills = teacherProfile?.Skills,
            Rate30Min = teacherProfile?.Rate30Min,
            Rate60Min = teacherProfile?.Rate60Min,
            AverageRating = teacherProfile?.AverageRating,
            TotalReviews = teacherProfile?.TotalReviews ?? 0
        });
    }
}
