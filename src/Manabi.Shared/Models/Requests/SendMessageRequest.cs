using System.ComponentModel.DataAnnotations;

namespace Manabi.Shared.Models.Requests;

public class SendMessageRequest
{
    [Required]
    public string RecipientId { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
