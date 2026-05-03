using System.Net.Http.Json;
using Manabi.Shared.Models;
using Manabi.Shared.Models.Requests;
using Manabi.Shared.Models.Responses;

namespace Manabi.Client.Services;

public class UserApiService(HttpClient http)
{
    public async Task<UserProfileResponse?> GetUserAsync(string userId)
        => await http.GetFromJsonAsync<UserProfileResponse>($"/api/users/{userId}");

    public async Task<bool> DeleteAccountAsync(AccountDeletionReason reason)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/users/me")
        {
            Content = JsonContent.Create(new DeleteAccountRequest(reason))
        };
        var response = await http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}
