using System.Net.Http.Json;
using System.Text.Json;
using Manabi.Shared.Models.Requests;
using Manabi.Shared.Models.Responses;
using Microsoft.JSInterop;

namespace Manabi.Client.Services;

public class AuthService(HttpClient http, IJSRuntime js, ManabiAuthStateProvider authStateProvider)
{
    private const string TokenKey = "manabi_token";

    public async Task<(AuthResponse? auth, string? error)> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/auth/login", request);
            if (!response.IsSuccessStatusCode)
                return (null, await ReadErrorAsync(response));

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null) return (null, "サーバーから予期しないレスポンスが返されました。");

            await js.InvokeVoidAsync("localStorage.setItem", TokenKey, auth.Token);
            await authStateProvider.NotifyAuthChangedAsync();
            return (auth, null);
        }
        catch
        {
            return (null, "サーバーに接続できませんでした。APIが起動しているか確認してください。");
        }
    }

    public async Task<(AuthResponse? auth, string? error)> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/auth/register", request);
            if (!response.IsSuccessStatusCode)
                return (null, await ReadErrorAsync(response));

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null) return (null, "サーバーから予期しないレスポンスが返されました。");

            await js.InvokeVoidAsync("localStorage.setItem", TokenKey, auth.Token);
            await authStateProvider.NotifyAuthChangedAsync();
            return (auth, null);
        }
        catch
        {
            return (null, "サーバーに接続できませんでした。APIが起動しているか確認してください。");
        }
    }

    public async Task LogoutAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await authStateProvider.NotifyAuthChangedAsync();
    }

    public async Task<string?> GetTokenAsync()
        => await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            // Identity が返す文字列配列をパース
            var errors = JsonSerializer.Deserialize<string[]>(body);
            if (errors is { Length: > 0 })
                return string.Join(" / ", errors);
            return body;
        }
        catch
        {
            return $"エラーが発生しました（{(int)response.StatusCode}）。";
        }
    }
}
