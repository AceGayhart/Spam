using MailKit.Net.Imap;

namespace Spam.Factories;

public interface IImapClientFactory
{
    ImapClient CreateImapClient();
}