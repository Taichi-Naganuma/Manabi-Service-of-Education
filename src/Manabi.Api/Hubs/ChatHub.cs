using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Manabi.Api.Data;
using Manabi.Api.Models;
using Manabi.Api.Services;

namespace Manabi.Api.Hubs;

[Authorize]
public class ChatHub(
    ChatService chatService,
    AppDbContext db,
    UserManager<AppUser> userManager,
    IEmailService emailService,
    ILogger<ChatHub> logger) : Hub
{
    private string CurrentUserId => Context.UserIdentifier!;

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, CurrentUserId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, CurrentUserId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string recipientId, string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
            throw new HubException("メッセージが無効です。");

        var message = await chatService.SaveMessageAsync(CurrentUserId, recipientId, content);

        // 送信者自身にも返す（送信確認）
        await Clients.Group(CurrentUserId).SendAsync("ReceiveMessage", message);

        // 受信者が接続中なら届ける
        await Clients.Group(recipientId).SendAsync("ReceiveMessage", message);

        try
        {
            var pref = await db.EmailNotificationPreferences.FindAsync(recipientId);
            if (pref?.NotifyOnSessionRequest ?? true)
            {
                var recipient = await userManager.FindByIdAsync(recipientId);
                var sender = await userManager.FindByIdAsync(CurrentUserId);
                if (recipient?.Email is not null && sender is not null)
                    await emailService.SendSessionRequestAsync(recipient.Email, recipient.DisplayName, sender.DisplayName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "メッセージ受信通知メール送信失敗 recipient={RecipientId}", recipientId);
        }
    }

    public async Task MarkRead(string senderId)
    {
        await chatService.MarkAsReadAsync(CurrentUserId, senderId);
    }
}
