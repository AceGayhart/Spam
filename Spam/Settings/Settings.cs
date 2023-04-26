namespace Spam;

public class Settings
{
    public MailServerSettings MailServer { get; set; } = null!;
    public SpamCopSettings SpamCop { get; set; } = null!;
}