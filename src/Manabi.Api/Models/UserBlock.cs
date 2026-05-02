namespace Manabi.Api.Models;

public class UserBlock
{
    public Guid Id { get; set; }
    public string BlockerUserId { get; set; } = "";
    public string BlockedUserId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? Blocker { get; set; }
    public AppUser? Blocked { get; set; }
}
