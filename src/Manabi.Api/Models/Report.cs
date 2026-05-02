namespace Manabi.Api.Models;

public class Report
{
    public Guid Id { get; set; }
    public string ReporterUserId { get; set; } = "";
    public string TargetType { get; set; } = ""; // "story" | "letter" | "profile" | "message"
    public string TargetId { get; set; } = "";
    public string Reason { get; set; } = "";
    public string? AdditionalText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; } = false;

    public AppUser? Reporter { get; set; }
}
