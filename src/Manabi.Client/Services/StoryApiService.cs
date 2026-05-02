using System.Net.Http.Json;

namespace Manabi.Client.Services;

public record StoryDto(
    Guid Id,
    string AuthorName,
    string AuthorUserId,
    string? Title,
    string Content,
    string? Category,
    DateTime PublishedAt,
    int ViewCount);

public record StoryEditDto(
    Guid Id,
    string? Title,
    string Content,
    string? Category,
    bool IsPublished,
    DateTime CreatedAt);

public record CreateStoryRequest(string Content, string? Title, string? Category, bool Publish = false);
public record UpdateStoryRequest(string? Title, string? Content, string? Category, bool? Publish);

public class StoryApiService(HttpClient http)
{
    public async Task<List<StoryDto>> GetStoriesAsync(string? category = null, string? authorId = null, int limit = 20)
    {
        var query = new List<string> { $"limit={limit}" };
        if (!string.IsNullOrEmpty(category)) query.Add($"category={Uri.EscapeDataString(category)}");
        if (!string.IsNullOrEmpty(authorId)) query.Add($"authorId={Uri.EscapeDataString(authorId)}");
        var url = "/api/stories?" + string.Join("&", query);
        return await http.GetFromJsonAsync<List<StoryDto>>(url) ?? [];
    }

    public async Task<List<StoryEditDto>> GetMyStoriesAsync()
        => await http.GetFromJsonAsync<List<StoryEditDto>>("/api/stories/mine") ?? [];

    public async Task<bool> CreateAsync(CreateStoryRequest req)
    {
        var res = await http.PostAsJsonAsync("/api/stories", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateStoryRequest req)
    {
        var res = await http.PatchAsJsonAsync($"/api/stories/{id}", req);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var res = await http.DeleteAsync($"/api/stories/{id}");
        return res.IsSuccessStatusCode;
    }
}
