namespace Manabi.Api.Models;

public class DirectMessage
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public AppUser Sender { get; set; } = null!;
    public string RecipientId { get; set; } = string.Empty;
    public AppUser Recipient { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
