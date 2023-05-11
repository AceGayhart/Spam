using MailKit.Net.Smtp;
using Spam.Configuration;
using Spam.Logging;

namespace Spam.Factories;

public class SmtpClientFactory : ISmtpClientFactory
{
    private readonly Settings _settings;

    public SmtpClientFactory(Settings settings)
    {
        _settings = settings;
    }

    public SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(new SerilogProtocolLogger());
        client.Connect(_settings.MailServer.Smtp.Host, _settings.MailServer.Smtp.Port);
        client.Authenticate(_settings.MailServer.Username, _settings.MailServer.Password);
        return client;
    }
}