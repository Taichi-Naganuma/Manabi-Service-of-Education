using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;
using Manabi.Api.Services;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/letters")]
[Authorize]
public class LettersController(
    AppDbContext db,
    UserManager<AppUser> userManager,
    IEmailService emailService) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    // POST /api/letters
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendLetterRequest req)
    {
        if (req.ToUserId == CurrentUserId)
            return BadRequest("自分自身には手紙を送れません。");

        if (req.Content.Length > 1000)
            return BadRequest("手紙は1000文字以内で書いてください。");

        var isBlocked = await db.UserBlocks.AnyAsync(b =>
            b.BlockerUserId == req.ToUserId && b.BlockedUserId == CurrentUserId);
        if (isBlocked)
            return BadRequest("この相手には手紙を送れません。");

        var letter = new WitnessLetter
        {
            Id = Guid.NewGuid(),
            FromUserId = CurrentUserId,
            ToUserId = req.ToUserId,
            Content = req.Content,
            SenderAllowsDisplay = req.SenderAllowsDisplay,
        };
        db.WitnessLetters.Add(letter);
        await db.SaveChangesAsync();

        // メール通知
        var pref = await db.EmailNotificationPreferences.FindAsync(req.ToUserId);
        if (pref?.NotifyOnLetterReceived ?? true)
        {
            var recipient = await userManager.FindByIdAsync(req.ToUserId);
            var sender = await userManager.FindByIdAsync(CurrentUserId);
            if (recipient?.Email is not null && sender is not null)
                await emailService.SendLetterReceivedAsync(recipient.Email, recipient.DisplayName, sender.DisplayName);
        }

        return Ok(new { letter.Id });
    }

    // GET /api/letters/received
    [HttpGet("received")]
    public async Task<IActionResult> GetReceived()
    {
        var letters = await db.WitnessLetters
            .Where(l => l.ToUserId == CurrentUserId && !l.IsDeleted)
            .Include(l => l.FromUser)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LetterResponse(
                l.Id,
                l.FromUser!.DisplayName,
                l.FromUserId,
                l.Content,
                l.CreatedAt,
                l.SenderAllowsDisplay,
                l.RecipientAllowsDisplay,
                l.IsPublic))
            .ToListAsync();
        return Ok(letters);
    }

    // GET /api/letters/sent
    [HttpGet("sent")]
    public async Task<IActionResult> GetSent()
    {
        var letters = await db.WitnessLetters
            .Where(l => l.FromUserId == CurrentUserId && !l.IsDeleted)
            .Include(l => l.ToUser)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LetterResponse(
                l.Id,
                l.ToUser!.DisplayName,
                l.ToUserId,
                l.Content,
                l.CreatedAt,
                l.SenderAllowsDisplay,
                l.RecipientAllowsDisplay,
                l.IsPublic))
            .ToListAsync();
        return Ok(letters);
    }

    // GET /api/letters/public/{userId}  — 証言セクション用（認証不要）
    [HttpGet("public/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(string userId, [FromQuery] int limit = 10)
    {
        var letters = await db.WitnessLetters
            .Where(l => l.ToUserId == userId && l.SenderAllowsDisplay && l.RecipientAllowsDisplay && !l.IsDeleted)
            .Include(l => l.FromUser)
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .Select(l => new PublicLetterResponse(
                l.Id,
                l.FromUser!.DisplayName,
                l.FromUserId,
                l.Content,
                l.CreatedAt))
            .ToListAsync();
        return Ok(letters);
    }

    // PATCH /api/letters/{id}/recipient-display
    [HttpPatch("{id:guid}/recipient-display")]
    public async Task<IActionResult> UpdateRecipientDisplay(Guid id, [FromBody] UpdateDisplayRequest req)
    {
        var letter = await db.WitnessLetters.FindAsync(id);
        if (letter is null) return NotFound();
        if (letter.ToUserId != CurrentUserId) return Forbid();

        letter.RecipientAllowsDisplay = req.AllowDisplay;
        await db.SaveChangesAsync();
        return Ok(new { letter.IsPublic });
    }

    // DELETE /api/letters/{id}  — 送信側のみ削除可
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var letter = await db.WitnessLetters.FindAsync(id);
        if (letter is null) return NotFound();
        if (letter.FromUserId != CurrentUserId) return Forbid();

        letter.IsDeleted = true;
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record SendLetterRequest(string ToUserId, string Content, bool SenderAllowsDisplay = true);
public record UpdateDisplayRequest(bool AllowDisplay);
public record LetterResponse(
    Guid Id,
    string OtherUserName,
    string OtherUserId,
    string Content,
    DateTime CreatedAt,
    bool SenderAllowsDisplay,
    bool RecipientAllowsDisplay,
    bool IsPublic);
public record PublicLetterResponse(
    Guid Id,
    string FromUserName,
    string FromUserId,
    string Content,
    DateTime CreatedAt);
