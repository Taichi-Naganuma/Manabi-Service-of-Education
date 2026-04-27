using System.Net.Http.Json;
using Manabi.Shared.Models.Responses;

namespace Manabi.Client.Services;

public class UserApiService(HttpClient http)
{
    public async Task<UserProfileResponse?> GetUserAsync(string userId)
        => await http.GetFromJsonAsync<UserProfileResponse>($"/api/users/{userId}");
}
