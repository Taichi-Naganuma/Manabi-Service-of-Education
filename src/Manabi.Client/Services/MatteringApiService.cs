using System.Net.Http.Json;

namespace Manabi.Client.Services;

public record MatteringStats(int UniquePartners, int LetterCount);

public class MatteringApiService(HttpClient http)
{
    public async Task<MatteringStats?> GetStatsAsync()
        => await http.GetFromJsonAsync<MatteringStats>("/api/mattering");

    public async Task<bool> BlockUserAsync(string targetUserId)
    {
        var res = await http.PostAsJsonAsync("/api/blocks", new { TargetUserId = targetUserId });
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ReportAsync(string targetType, string targetId, string reason, string? additionalText = null)
    {
        var res = await http.PostAsJsonAsync("/api/reports", new
        {
            TargetType = targetType,
            TargetId = targetId,
            Reason = reason,
            AdditionalText = additionalText
        });
        return res.IsSuccessStatusCode;
    }
}
