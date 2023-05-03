namespace Spam.Configuration;

public class Settings
{
    public List<string>? AllowedSenders { get; set; }
    public MailServerSettings MailServer { get; set; } = null!;
    public SpamCopSettings SpamCop { get; set; } = null!;
}