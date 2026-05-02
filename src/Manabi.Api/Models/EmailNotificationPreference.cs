namespace Manabi.Api.Models;

public class EmailNotificationPreference
{
    public string UserId { get; set; } = "";
    public bool NotifyOnLetterReceived { get; set; } = true;
    public bool NotifyOnSessionRequest { get; set; } = true;

    public AppUser? User { get; set; }
}
