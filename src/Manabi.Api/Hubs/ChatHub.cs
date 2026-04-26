using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Manabi.Api.Services;

namespace Manabi.Api.Hubs;

[Authorize]
public class ChatHub(ChatService chatService) : Hub
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
    }

    public async Task MarkRead(string senderId)
    {
        await chatService.MarkAsReadAsync(CurrentUserId, senderId);
    }
}
