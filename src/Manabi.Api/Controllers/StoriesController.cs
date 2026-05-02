using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;

namespace Manabi.Api.Controllers;

[ApiController]
[Route("api/stories")]
public class StoriesController(AppDbContext db) : ControllerBase
{
    private string CurrentUserId => User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    // GET /api/stories?category=X&limit=20&authorId=Y
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] string? category,
        [FromQuery] string? authorId,
        [FromQuery] int limit = 20)
    {
        var query = db.StorySnippets
            .Where(s => s.IsPublished)
            .Include(s => s.Author)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(s => s.Category == category);

        if (!string.IsNullOrEmpty(authorId))
            query = query.Where(s => s.AuthorUserId == authorId);

        var stories = await query
            .OrderByDescending(s => s.PublishedAt)
            .Take(limit)
            .Select(s => new StoryResponse(
                s.Id,
                s.Author!.DisplayName,
                s.AuthorUserId,
                s.Title,
                s.Content,
                s.Category,
                s.PublishedAt ?? s.CreatedAt,
                s.ViewCount))
            .ToListAsync();

        return Ok(stories);
    }

    // GET /api/stories/{id}
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var story = await db.StorySnippets
            .Include(s => s.Author)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsPublished);

        if (story is null) return NotFound();

        story.ViewCount++;
        await db.SaveChangesAsync();

        return Ok(new StoryResponse(
            story.Id,
            story.Author!.DisplayName,
            story.AuthorUserId,
            story.Title,
            story.Content,
            story.Category,
            story.PublishedAt ?? story.CreatedAt,
            story.ViewCount));
    }

    // POST /api/stories
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateStoryRequest req)
    {
        if (req.Content.Length < 50)
            return BadRequest("本文は50文字以上で書いてください。");

        // 1人5本まで
        var count = await db.StorySnippets.CountAsync(s => s.AuthorUserId == CurrentUserId);
        if (count >= 5)
            return BadRequest("Story Cards は1人5本までです。");

        var story = new StorySnippet
        {
            Id = Guid.NewGuid(),
            AuthorUserId = CurrentUserId,
            Title = req.Title,
            Content = req.Content,
            Category = req.Category,
            IsPublished = req.Publish,
            CreatedAt = DateTime.UtcNow,
            PublishedAt = req.Publish ? DateTime.UtcNow : null,
        };
        db.StorySnippets.Add(story);
        await db.SaveChangesAsync();

        return Ok(new { story.Id });
    }

    // PATCH /api/stories/{id}
    [HttpPatch("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoryRequest req)
    {
        var story = await db.StorySnippets.FindAsync(id);
        if (story is null) return NotFound();
        if (story.AuthorUserId != CurrentUserId) return Forbid();

        if (req.Title is not null) story.Title = req.Title;
        if (req.Content is not null)
        {
            if (req.Content.Length < 50) return BadRequest("本文は50文字以上で書いてください。");
            story.Content = req.Content;
        }
        if (req.Category is not null) story.Category = req.Category;
        if (req.Publish.HasValue)
        {
            story.IsPublished = req.Publish.Value;
            if (req.Publish.Value && story.PublishedAt is null)
                story.PublishedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Ok(new { story.Id, story.IsPublished });
    }

    // DELETE /api/stories/{id}
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var story = await db.StorySnippets.FindAsync(id);
        if (story is null) return NotFound();
        if (story.AuthorUserId != CurrentUserId) return Forbid();

        db.StorySnippets.Remove(story);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/stories/mine
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var stories = await db.StorySnippets
            .Where(s => s.AuthorUserId == CurrentUserId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StoryEditResponse(
                s.Id, s.Title, s.Content, s.Category, s.IsPublished, s.CreatedAt))
            .ToListAsync();
        return Ok(stories);
    }
}

public record CreateStoryRequest(string Content, string? Title, string? Category, bool Publish = false);
public record UpdateStoryRequest(string? Title, string? Content, string? Category, bool? Publish);
public record StoryResponse(
    Guid Id,
    string AuthorName,
    string AuthorUserId,
    string? Title,
    string Content,
    string? Category,
    DateTime PublishedAt,
    int ViewCount);
public record StoryEditResponse(
    Guid Id,
    string? Title,
    string Content,
    string? Category,
    bool IsPublished,
    DateTime CreatedAt);
