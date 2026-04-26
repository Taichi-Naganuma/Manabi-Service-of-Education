using Manabi.Shared.Models;

namespace Manabi.Api.Models;

public class Session
{
    public int Id { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public AppUser Teacher { get; set; } = null!;
    public string StudentId { get; set; } = string.Empty;
    public AppUser Student { get; set; } = null!;

    public SessionStatus Status { get; set; } = SessionStatus.Requested;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }

    public int Price { get; set; }
    public int PlatformFee { get; set; }
    public string? StripePaymentIntentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Message> Messages { get; set; } = [];
    public Review? Review { get; set; }
}
