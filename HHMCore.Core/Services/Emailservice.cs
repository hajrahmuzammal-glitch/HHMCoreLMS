using HHMCore.Core.Common;
using HHMCore.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HHMCore.Core.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendCredentialsAsync(
        string to, string fullName, string email, string password, string role)
    {
        // TODO: Before production — replace password with a password-reset link.
        // Sending plain-text passwords over email is acceptable only during development.
        var subject = "Welcome to HHMCore LMS — Your Login Credentials";
        var htmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #2c3e50;'>Welcome to HHMCore LMS</h2>
                <p>Hello <strong>{fullName}</strong>,</p>
                <p>Your {role} account has been created. Here are your login credentials:</p>
                <table style='background:#f8f9fa; padding:15px; border-radius:5px; width:100%;'>
                    <tr>
                        <td><strong>Email:</strong></td>
                        <td>{email}</td>
                    </tr>
                    <tr>
                        <td><strong>Password:</strong></td>
                        <td>{password}</td>
                    </tr>
                    <tr>
                        <td><strong>Role:</strong></td>
                        <td>{role}</td>
                    </tr>
                </table>
                <p style='color:#e74c3c;'><strong>Please change your password immediately after your first login.</strong></p>
                <p>Best regards,<br/><strong>HHMCore LMS Administration</strong></p>
            </body>
            </html>";

        await SendAsync(to, subject, htmlBody);
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            // Email failure must NEVER crash the request.
            // Student/Teacher is still created — email is non-critical path.
            _logger.LogError(ex, "Failed to send email to {To}. Subject: {Subject}", to, subject);
        }
    }
}