using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manabi.Api.Services;
using Manabi.Shared.Models.Requests;
using Manabi.Shared.Models.Responses;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeachersController(TeacherService teacherService) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    // GET /api/teachers?skill=Unity&maxRate=2000&sortBy=rating
    [HttpGet]
    public async Task<ActionResult<List<TeacherProfileResponse>>> Search(
        [FromQuery] string? skill,
        [FromQuery] int? maxRate,
        [FromQuery] string sortBy = "rating")
    {
        var results = await teacherService.SearchAsync(skill, maxRate, sortBy);
        return Ok(results);
    }

    // GET /api/teachers/{userId}
    [HttpGet("{userId}")]
    public async Task<ActionResult<TeacherProfileResponse>> GetById(string userId)
    {
        var profile = await teacherService.GetByUserIdAsync(userId);
        if (profile is null) return NotFound("先生が見つかりません。");
        return Ok(profile);
    }

    // POST /api/teachers/profile  （先生プロフィール新規作成）
    [Authorize]
    [HttpPost("profile")]
    public async Task<ActionResult<TeacherProfileResponse>> Create(CreateTeacherProfileRequest req)
    {
        var (result, error) = await teacherService.CreateAsync(CurrentUserId, req);
        if (error is not null) return Conflict(error);
        return CreatedAtAction(nameof(GetById), new { userId = result!.UserId }, result);
    }

    // PATCH /api/teachers/profile  （先生プロフィール更新）
    [Authorize]
    [HttpPatch("profile")]
    public async Task<ActionResult<TeacherProfileResponse>> Update(UpdateTeacherProfileRequest req)
    {
        var (result, error) = await teacherService.UpdateAsync(CurrentUserId, req);
        if (error is not null) return NotFound(error);
        return Ok(result);
    }
}
