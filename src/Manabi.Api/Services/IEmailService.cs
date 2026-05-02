namespace Manabi.Api.Services;

public interface IEmailService
{
    Task SendLetterReceivedAsync(string toEmail, string toName, string fromName);
    Task SendSessionRequestAsync(string toEmail, string toName, string fromName);
}
