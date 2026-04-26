using System.Net.Http.Json;
using System.Text.Json;
using Manabi.Shared.Models.Requests;
using Manabi.Shared.Models.Responses;
using Microsoft.JSInterop;

namespace Manabi.Client.Services;

public class AuthService(HttpClient http, IJSRuntime js, ManabiAuthStateProvider authStateProvider)
{
    private const string TokenKey = "manabi_token";

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null) return null;

        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, auth.Token);
        await authStateProvider.NotifyAuthChangedAsync();
        return auth;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/auth/register", request);
        if (!response.IsSuccessStatusCode) return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null) return null;

        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, auth.Token);
        await authStateProvider.NotifyAuthChangedAsync();
        return auth;
    }

    public async Task LogoutAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await authStateProvider.NotifyAuthChangedAsync();
    }

    public async Task<string?> GetTokenAsync()
        => await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
}
