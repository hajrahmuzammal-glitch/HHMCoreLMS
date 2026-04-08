namespace HHMCore.Core.Common;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;  // smtp.gmail.com
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;  // your Gmail address
    public string Password { get; set; } = string.Empty;  // Gmail App Password
    public string FromAddress { get; set; } = string.Empty;  // sender email
    public string FromName { get; set; } = "HHMCore LMS"; // sender display name
}