namespace Manabi.Api.Models;

public class SelfReflection
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string PartnerUserId { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? User { get; set; }
    public AppUser? PartnerUser { get; set; }
}
