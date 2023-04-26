using MailKit.Net.Smtp;

namespace Spam;

public class SmtpClientFactory : ISmtpClientFactory
{
    public SmtpClient CreateSmtpClient(Settings settings)
    {
        var client = new SmtpClient(new SerilogProtocolLogger());
        client.Connect(settings.MailServer.Smtp.Host, settings.MailServer.Smtp.Port);
        client.Authenticate(settings.MailServer.Username, settings.MailServer.Password);
        return client;
    }
}