namespace Manabi.Api.Models;

public class StorySnippet
{
    public Guid Id { get; set; }
    public string AuthorUserId { get; set; } = "";
    public string? Title { get; set; }
    public string Content { get; set; } = "";
    public string? Category { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; } = 0;

    public AppUser? Author { get; set; }
}
