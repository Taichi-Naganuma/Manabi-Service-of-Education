namespace Manabi.Shared.Models.Responses;

public class MessageResponse
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsOwn { get; set; }
    public bool IsSystemMessage { get; set; }
    public bool IsSenderDeleted { get; set; }
}
