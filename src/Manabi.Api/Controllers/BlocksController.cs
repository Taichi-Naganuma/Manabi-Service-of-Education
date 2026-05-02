using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/blocks")]
[Authorize]
public class BlocksController(AppDbContext db) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    // POST /api/blocks
    [HttpPost]
    public async Task<IActionResult> Block([FromBody] BlockRequest req)
    {
        if (req.TargetUserId == CurrentUserId)
            return BadRequest("自分自身をブロックできません。");

        var exists = await db.UserBlocks.AnyAsync(b =>
            b.BlockerUserId == CurrentUserId && b.BlockedUserId == req.TargetUserId);
        if (exists) return Conflict("既にブロック済みです。");

        db.UserBlocks.Add(new UserBlock
        {
            Id = Guid.NewGuid(),
            BlockerUserId = CurrentUserId,
            BlockedUserId = req.TargetUserId,
        });
        await db.SaveChangesAsync();
        return Ok();
    }

    // DELETE /api/blocks/{targetUserId}
    [HttpDelete("{targetUserId}")]
    public async Task<IActionResult> Unblock(string targetUserId)
    {
        var block = await db.UserBlocks.FirstOrDefaultAsync(b =>
            b.BlockerUserId == CurrentUserId && b.BlockedUserId == targetUserId);
        if (block is null) return NotFound();

        db.UserBlocks.Remove(block);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/blocks
    [HttpGet]
    public async Task<IActionResult> GetBlocked()
    {
        var blocked = await db.UserBlocks
            .Where(b => b.BlockerUserId == CurrentUserId)
            .Include(b => b.Blocked)
            .Select(b => new { b.BlockedUserId, b.Blocked!.DisplayName, b.CreatedAt })
            .ToListAsync();
        return Ok(blocked);
    }
}

public record BlockRequest(string TargetUserId);
