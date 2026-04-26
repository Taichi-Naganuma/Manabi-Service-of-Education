using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Manabi.Client.Services;

public class ManabiAuthStateProvider(IJSRuntime js) : AuthenticationStateProvider
{
    private const string TokenKey = "manabi_token";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaimsFromJwt(token);
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is not null)
        {
            var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
            if (exp < DateTimeOffset.UtcNow)
            {
                await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task NotifyAuthChangedAsync()
    {
        var state = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bytes);

        var claims = new List<Claim>();
        if (json is null) return claims;

        foreach (var (key, value) in json)
        {
            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                    claims.Add(new Claim(key, item.GetString() ?? ""));
            }
            else
            {
                claims.Add(new Claim(key, value.ToString()));
            }
        }
        return claims;
    }
}
