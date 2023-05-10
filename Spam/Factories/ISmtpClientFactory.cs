using MailKit.Net.Smtp;

namespace Spam.Factories;

public interface ISmtpClientFactory
{
    SmtpClient CreateSmtpClient();
}