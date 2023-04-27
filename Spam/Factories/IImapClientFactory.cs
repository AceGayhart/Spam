using MailKit.Net.Imap;
using Spam.Configuration;

namespace Spam.Factories;

public interface IImapClientFactory
{
    ImapClient CreateImapClient(Settings settings);
}