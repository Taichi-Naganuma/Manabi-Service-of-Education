using Microsoft.EntityFrameworkCore;
using Manabi.Api.Data;
using Manabi.Api.Models;
using Manabi.Shared.Models.Responses;

namespace Manabi.Api.Services;

public class ChatService(AppDbContext db)
{
    public async Task<List<MessageResponse>> GetConversationAsync(
        string userId, string otherUserId, int skip = 0, int take = 50)
    {
        return await db.DirectMessages
            .Include(m => m.Sender)
            .Where(m =>
                (m.SenderId == userId && m.RecipientId == otherUserId) ||
                (m.SenderId == otherUserId && m.RecipientId == userId))
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .OrderBy(m => m.SentAt)
            .Select(m => ToResponse(m, userId))
            .ToListAsync();
    }

    public async Task<MessageResponse> SaveMessageAsync(
        string senderId, string recipientId, string content)
    {
        var message = new DirectMessage
        {
            SenderId = senderId,
            RecipientId = recipientId,
            Content = content
        };

        db.DirectMessages.Add(message);
        await db.SaveChangesAsync();
        await db.Entry(message).Reference(m => m.Sender).LoadAsync();

        return ToResponse(message, senderId);
    }

    public async Task MarkAsReadAsync(string recipientId, string senderId)
    {
        await db.DirectMessages
            .Where(m => m.SenderId == senderId && m.RecipientId == recipientId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
    }

    public async Task<List<ConversationResponse>> GetConversationsAsync(string userId)
    {
        var messages = await db.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .ToListAsync();

        return messages
            .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
            .Select(g =>
            {
                var last = g.OrderByDescending(m => m.SentAt).First();
                var otherUser = g.Key == last.SenderId ? last.Sender : last.Recipient;
                return new ConversationResponse
                {
                    OtherUserId = g.Key,
                    OtherUserDisplayName = otherUser.DisplayName,
                    OtherUserAvatarUrl = otherUser.AvatarUrl,
                    LastMessage = last.Content,
                    LastMessageAt = last.SentAt,
                    UnreadCount = g.Count(m => m.SenderId == g.Key && m.RecipientId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();
    }

    private static MessageResponse ToResponse(DirectMessage m, string currentUserId) => new()
    {
        Id = m.Id,
        SenderId = m.SenderId,
        SenderDisplayName = m.Sender.DisplayName,
        RecipientId = m.RecipientId,
        Content = m.Content,
        SentAt = m.SentAt,
        IsOwn = m.SenderId == currentUserId
    };
}
