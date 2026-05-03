using Microsoft.AspNetCore.Mvc;
using Manabi.Api.Data;
using Manabi.Api.Models;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController(AppDbContext db) : ControllerBase
{
    private string? CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    [HttpPost("event")]
    public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest req)
    {
        db.UserActionLogs.Add(new UserActionLog
        {
            UserId = CurrentUserId,
            EventName = req.EventName,
            Page = req.Page,
            TargetId = req.TargetId,
            Meta = req.Meta,
        });
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record TrackEventRequest(string EventName, string? Page, string? TargetId, string? Meta);
