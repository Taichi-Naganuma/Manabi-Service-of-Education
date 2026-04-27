using System.Net.Http.Json;
using Manabi.Shared.Models.Responses;

namespace Manabi.Client.Services;

public class ChatApiService(HttpClient http)
{
    public async Task<List<ConversationResponse>> GetConversationsAsync()
        => await http.GetFromJsonAsync<List<ConversationResponse>>("/api/messages/conversations") ?? [];

    public async Task<List<MessageResponse>> GetMessagesAsync(string otherUserId)
        => await http.GetFromJsonAsync<List<MessageResponse>>($"/api/messages/{otherUserId}") ?? [];

    public async Task MarkReadAsync(string senderId)
        => await http.PostAsync($"/api/messages/{senderId}/read", null);
}
