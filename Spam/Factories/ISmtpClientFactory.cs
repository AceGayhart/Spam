using MailKit.Net.Smtp;
using Spam.Configuration;

namespace Spam.Factories;

public interface ISmtpClientFactory
{
    SmtpClient CreateSmtpClient(Settings settings);
}