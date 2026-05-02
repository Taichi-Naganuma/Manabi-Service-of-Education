using SendGrid;
using SendGrid.Helpers.Mail;

namespace Manabi.Api.Services;

public class SendGridEmailService(IConfiguration config, ILogger<SendGridEmailService> logger) : IEmailService
{
    private readonly string _apiKey = config["SendGrid:ApiKey"] ?? "";
    private readonly string _fromEmail = config["SendGrid:FromEmail"] ?? "noreply@mananect.com";
    private readonly string _fromName = config["SendGrid:FromName"] ?? "Mananect";
    private readonly string _siteUrl = config["ClientUrl"]?.Split(',')[0] ?? "http://localhost:5048";

    public async Task SendLetterReceivedAsync(string toEmail, string toName, string fromName)
    {
        var subject = $"✉️ {fromName}さんから手紙が届きました";
        var html = $"""
            <p>{toName}さん、</p>
            <p>{fromName}さんからあなたへの手紙が届きました。</p>
            <p><a href="{_siteUrl}/my-letters">手紙を読む</a></p>
            <hr>
            <p style="font-size:12px;color:#888;">
              配信停止は<a href="{_siteUrl}/settings/notifications">通知設定</a>から行えます。<br>
              Mananect（個人事業主 Zhang）
            </p>
            """;
        await SendAsync(toEmail, toName, subject, html);
    }

    public async Task SendSessionRequestAsync(string toEmail, string toName, string fromName)
    {
        var subject = $"{fromName}さんがあなたの話を聞きたがっています";
        var html = $"""
            <p>{toName}さん、</p>
            <p>{fromName}さんがあなたにメッセージを送りました。</p>
            <p><a href="{_siteUrl}/messages">確認する</a></p>
            <hr>
            <p style="font-size:12px;color:#888;">
              配信停止は<a href="{_siteUrl}/settings/notifications">通知設定</a>から行えます。<br>
              Mananect（個人事業主 Zhang）
            </p>
            """;
        await SendAsync(toEmail, toName, subject, html);
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string html)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            logger.LogWarning("SendGrid API Key が未設定のためメール送信をスキップ: {Subject}", subject);
            return;
        }

        try
        {
            var client = new SendGridClient(_apiKey);
            var msg = MailHelper.CreateSingleEmail(
                new EmailAddress(_fromEmail, _fromName),
                new EmailAddress(toEmail, toName),
                subject,
                plainTextContent: null,
                htmlContent: html);
            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("SendGrid 送信失敗: StatusCode={Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "メール送信中に例外が発生しました: {Subject}", subject);
        }
    }
}
