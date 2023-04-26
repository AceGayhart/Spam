using MailKit.Net.Imap;

namespace Spam;

public class ImapClientFactory : IImapClientFactory
{
    public ImapClient CreateImapClient(Settings settings)
    {
        var client = new ImapClient(new SerilogProtocolLogger());
        client.Connect(
            settings.MailServer.Imap.Host,
            settings.MailServer.Imap.Port,
            MailKit.Security.SecureSocketOptions.SslOnConnect);
        client.Authenticate(settings.MailServer.Username, settings.MailServer.Password);
        return client;
    }
}