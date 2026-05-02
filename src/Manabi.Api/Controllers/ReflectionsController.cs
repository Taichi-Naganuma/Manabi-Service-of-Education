using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/reflections")]
[Authorize]
public class ReflectionsController(AppDbContext db) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    // POST /api/reflections
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReflectionRequest req)
    {
        db.SelfReflections.Add(new SelfReflection
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,
            PartnerUserId = req.PartnerUserId,
            Content = req.Content,
        });
        await db.SaveChangesAsync();
        return Ok();
    }

    // GET /api/reflections
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var reflections = await db.SelfReflections
            .Where(r => r.UserId == CurrentUserId)
            .Include(r => r.PartnerUser)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReflectionResponse(
                r.Id,
                r.PartnerUser!.DisplayName,
                r.PartnerUserId,
                r.Content,
                r.CreatedAt))
            .ToListAsync();
        return Ok(reflections);
    }
}

// GET /api/mattering — 本人専用のMattering Counter
[ApiController]
[Route("api/mattering")]
[Authorize]
public class MatteringController(AppDbContext db) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var uniqueConversationPartners = await db.DirectMessages
            .Where(m => m.SenderId == CurrentUserId || m.RecipientId == CurrentUserId)
            .Select(m => m.SenderId == CurrentUserId ? m.RecipientId : m.SenderId)
            .Distinct()
            .CountAsync();

        var letterCount = await db.WitnessLetters
            .CountAsync(l => l.ToUserId == CurrentUserId && !l.IsDeleted);

        return Ok(new { UniquePartners = uniqueConversationPartners, LetterCount = letterCount });
    }
}

public record CreateReflectionRequest(string PartnerUserId, string Content);
public record ReflectionResponse(Guid Id, string PartnerName, string PartnerUserId, string Content, DateTime CreatedAt);
