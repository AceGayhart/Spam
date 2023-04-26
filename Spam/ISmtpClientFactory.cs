using MailKit.Net.Smtp;

namespace Spam;

public interface ISmtpClientFactory
{
    SmtpClient CreateSmtpClient(Settings settings);
}
