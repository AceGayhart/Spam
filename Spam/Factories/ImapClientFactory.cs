using MailKit.Net.Imap;
using Spam.Configuration;
using Spam.Logging;

namespace Spam.Factories;

public class ImapClientFactory : IImapClientFactory
{
    private readonly Settings _settings;

    public ImapClientFactory(Settings settings)
    {
        _settings = settings;
    }

    public ImapClient CreateImapClient()
    {
        var client = new ImapClient(new SerilogProtocolLogger());
        client.Connect(
            _settings.MailServer.Imap.Host,
            _settings.MailServer.Imap.Port,
            MailKit.Security.SecureSocketOptions.SslOnConnect);
        client.Authenticate(_settings.MailServer.Username, _settings.MailServer.Password);
        return client;
    }
}