using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manabi.Api.Data;
using Manabi.Api.Models;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(AppDbContext db) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    // GET /api/notifications/preferences
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var pref = await db.EmailNotificationPreferences.FindAsync(CurrentUserId)
            ?? new EmailNotificationPreference { UserId = CurrentUserId };
        return Ok(new NotificationPrefResponse(pref.NotifyOnLetterReceived, pref.NotifyOnSessionRequest));
    }

    // PUT /api/notifications/preferences
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] NotificationPrefResponse req)
    {
        var pref = await db.EmailNotificationPreferences.FindAsync(CurrentUserId);
        if (pref is null)
        {
            pref = new EmailNotificationPreference { UserId = CurrentUserId };
            db.EmailNotificationPreferences.Add(pref);
        }

        pref.NotifyOnLetterReceived = req.NotifyOnLetterReceived;
        pref.NotifyOnSessionRequest = req.NotifyOnSessionRequest;
        await db.SaveChangesAsync();
        return Ok(new NotificationPrefResponse(pref.NotifyOnLetterReceived, pref.NotifyOnSessionRequest));
    }
}

public record NotificationPrefResponse(bool NotifyOnLetterReceived, bool NotifyOnSessionRequest);
