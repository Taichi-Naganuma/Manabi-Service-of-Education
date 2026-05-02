using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manabi.Api.Data;
using Manabi.Api.Models;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(AppDbContext db) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    // POST /api/reports
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitReportRequest req)
    {
        var validTypes = new[] { "story", "letter", "profile", "message" };
        if (!validTypes.Contains(req.TargetType))
            return BadRequest("不正な対象タイプです。");

        var validReasons = new[] { "inappropriate", "harassment", "spam", "other" };
        if (!validReasons.Contains(req.Reason))
            return BadRequest("不正な通報理由です。");

        db.Reports.Add(new Report
        {
            Id = Guid.NewGuid(),
            ReporterUserId = CurrentUserId,
            TargetType = req.TargetType,
            TargetId = req.TargetId,
            Reason = req.Reason,
            AdditionalText = req.AdditionalText,
        });
        await db.SaveChangesAsync();
        return Ok();
    }
}

public record SubmitReportRequest(
    string TargetType,
    string TargetId,
    string Reason,
    string? AdditionalText);
