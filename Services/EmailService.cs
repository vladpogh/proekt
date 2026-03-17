using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace proekt.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _config;

    public EmailService(ILogger<EmailService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var port = int.Parse(_config["EmailSettings:Port"] ?? "587");
        var senderName = _config["EmailSettings:SenderName"];
        var senderEmail = _config["EmailSettings:SenderEmail"];
        var password = _config["EmailSettings:Password"];

        if (string.IsNullOrEmpty(senderEmail) || password == "YOUR_APP_PASSWORD")
        {
            _logger.LogWarning("SMTP settings not configured. Logging email instead:\nTo: {To}\nSubject: {Subject}\nBody: {Body}", to, subject, body);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}
