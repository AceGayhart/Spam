namespace Spam;

public class MailServerSettings
{
    public string DisplayName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public ServerSettings Imap { get; set; } = null!;
    public string Password { get; set; } = null!;
    public ServerSettings Smtp { get; set; } = null!;
    public string Username { get; set; } = null!;
}