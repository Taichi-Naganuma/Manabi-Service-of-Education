namespace Manabi.Api.Models;

public class WitnessLetter
{
    public Guid Id { get; set; }
    public string FromUserId { get; set; } = "";
    public string ToUserId { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool SenderAllowsDisplay { get; set; } = true;
    public bool RecipientAllowsDisplay { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    public bool IsPublic => SenderAllowsDisplay && RecipientAllowsDisplay && !IsDeleted;

    public AppUser? FromUser { get; set; }
    public AppUser? ToUser { get; set; }
}
