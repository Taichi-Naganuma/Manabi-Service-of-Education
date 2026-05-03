using System.Net.Http.Json;

namespace Manabi.Client.Services;

public class AnalyticsService(HttpClient http)
{
    public void Track(string eventName, string? page = null, string? targetId = null, string? meta = null)
    {
        _ = http.PostAsJsonAsync("/api/analytics/event", new
        {
            EventName = eventName,
            Page = page,
            TargetId = targetId,
            Meta = meta,
        });
    }
}
