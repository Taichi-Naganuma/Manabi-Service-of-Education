using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;
using Manabi.Api.Services;
using Manabi.Shared.Models.Requests;
using Manabi.Shared.Models.Responses;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    UserManager<AppUser> userManager,
    AppDbContext db,
    AccountDeletionService deletionService) : ControllerBase
{
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfileResponse>> GetUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        if (user.IsDeleted)
        {
            return Ok(new UserProfileResponse
            {
                UserId = user.Id,
                DisplayName = user.DisplayName,
                Bio = string.Empty,
                AvatarUrl = null,
                IsTeacher = false,
                IsDeleted = true,
                TotalReviews = 0
            });
        }

        var teacherProfile = await db.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == userId);

        return Ok(new UserProfileResponse
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            IsTeacher = teacherProfile is not null,
            IsDeleted = false,
            Skills = teacherProfile?.Skills,
            Categories = teacherProfile?.Categories,
            Rate30Min = teacherProfile?.Rate30Min,
            Rate60Min = teacherProfile?.Rate60Min,
            AverageRating = teacherProfile?.AverageRating,
            TotalReviews = teacherProfile?.TotalReviews ?? 0
        });
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe([FromBody] DeleteAccountRequest req)
    {
        var userId = User.FindFirst("sub")?.Value
                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var ok = await deletionService.DeleteAsync(userId, req.Reason);
        if (!ok) return NotFound();
        return NoContent();
    }
}
