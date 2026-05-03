namespace Manabi.Api.Models;

public class UserActionLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? UserId { get; set; }
    public string EventName { get; set; } = "";
    public string? Page { get; set; }
    public string? TargetId { get; set; }
    public string? Meta { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
