namespace HHMCore.Core.Interfaces;

public interface IEmailService
{
    // Sends login credentials to a newly created user (student or teacher)
    // TODO: Before production — replace password param with a password-reset token + link
    Task SendCredentialsAsync(string to, string fullName, string email, string password, string role);

    // Generic email sender — for future use (fee receipts, reminders, etc.)
    Task SendAsync(string to, string subject, string htmlBody);
}