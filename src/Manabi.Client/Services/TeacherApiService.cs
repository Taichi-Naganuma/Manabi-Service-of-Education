using System.Net.Http.Json;
using Manabi.Shared.Models.Requests;
using Manabi.Shared.Models.Responses;

namespace Manabi.Client.Services;

public class TeacherApiService(HttpClient http)
{
    public async Task<List<TeacherProfileResponse>> GetTeachersAsync(
        string? skill = null, int? maxRate = null, string? sortBy = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrEmpty(skill)) query.Add($"skill={Uri.EscapeDataString(skill)}");
        if (maxRate.HasValue) query.Add($"maxRate={maxRate}");
        if (!string.IsNullOrEmpty(sortBy)) query.Add($"sortBy={sortBy}");

        var url = "/api/teachers" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await http.GetFromJsonAsync<List<TeacherProfileResponse>>(url) ?? [];
    }

    public async Task<TeacherProfileResponse?> GetTeacherAsync(string userId)
        => await http.GetFromJsonAsync<TeacherProfileResponse>($"/api/teachers/{userId}");

    public async Task<TeacherProfileResponse?> GetMyProfileAsync()
    {
        var response = await http.GetAsync("/api/teachers/profile/me");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<TeacherProfileResponse>();
    }

    public async Task<bool> CreateProfileAsync(CreateTeacherProfileRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/teachers/profile", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProfileAsync(UpdateTeacherProfileRequest request)
    {
        var response = await http.PatchAsJsonAsync("/api/teachers/profile", request);
        return response.IsSuccessStatusCode;
    }
}
