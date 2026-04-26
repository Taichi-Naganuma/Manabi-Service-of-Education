using Microsoft.JSInterop;

namespace Manabi.Client.Services;

public class AuthorizationMessageHandler(IJSRuntime js) : DelegatingHandler
{
    private const string TokenKey = "manabi_token";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await js.InvokeAsync<string?>("localStorage.getItem", TokenKey, cancellationToken);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
