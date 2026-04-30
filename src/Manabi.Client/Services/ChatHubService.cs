using Manabi.Shared.Models.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Manabi.Client.Services;

public class ChatHubService(IConfiguration config, IJSRuntime js, NavigationManager nav) : IAsyncDisposable
{
    private const string TokenKey = "manabi_token";
    private HubConnection? _hub;

    public event Action<MessageResponse>? OnMessageReceived;

    public async Task ConnectAsync()
    {
        if (_hub?.State == HubConnectionState.Connected) return;

        var token = await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        var baseUrl = config["ApiBaseUrl"] ?? nav.BaseUri.TrimEnd('/');

        _hub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/chat", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _hub.On<MessageResponse>("ReceiveMessage", msg => OnMessageReceived?.Invoke(msg));

        await _hub.StartAsync();
    }

    public async Task SendMessageAsync(string recipientId, string content)
    {
        if (_hub is null) return;
        await _hub.InvokeAsync("SendMessage", recipientId, content);
    }

    public async Task MarkReadAsync(string senderId)
    {
        if (_hub is null) return;
        await _hub.InvokeAsync("MarkRead", senderId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
            await _hub.DisposeAsync();
    }
}
