using System.Net.Http.Json;

namespace Manabi.Client.Services;

public record LetterDto(
    Guid Id,
    string OtherUserName,
    string OtherUserId,
    string Content,
    DateTime CreatedAt,
    bool SenderAllowsDisplay,
    bool RecipientAllowsDisplay,
    bool IsPublic);

public record SendLetterRequest(string ToUserId, string Content, bool SenderAllowsDisplay = true);

public class LetterApiService(HttpClient http)
{
    public async Task<bool> SendAsync(SendLetterRequest req)
    {
        var res = await http.PostAsJsonAsync("/api/letters", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<LetterDto>> GetReceivedAsync()
        => await http.GetFromJsonAsync<List<LetterDto>>("/api/letters/received") ?? [];

    public async Task<List<LetterDto>> GetSentAsync()
        => await http.GetFromJsonAsync<List<LetterDto>>("/api/letters/sent") ?? [];

    public async Task<bool> UpdateRecipientDisplayAsync(Guid letterId, bool allowDisplay)
    {
        var res = await http.PatchAsJsonAsync($"/api/letters/{letterId}/recipient-display", new { AllowDisplay = allowDisplay });
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid letterId)
    {
        var res = await http.DeleteAsync($"/api/letters/{letterId}");
        return res.IsSuccessStatusCode;
    }
}
