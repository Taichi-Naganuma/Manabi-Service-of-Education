using Manabi.Shared.Models;

namespace Manabi.Api.Models;

public class AccountDeletionLog
{
    public int Id { get; set; }
    public AccountDeletionReason Reason { get; set; }
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    public int AccountAgeDays { get; set; }
    public bool HadTeacherProfile { get; set; }
    public bool HadSentLetters { get; set; }
    public bool HadReceivedLetters { get; set; }
}
