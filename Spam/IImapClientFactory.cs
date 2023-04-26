using MailKit.Net.Imap;

namespace Spam;

public interface IImapClientFactory
{
    ImapClient CreateImapClient(Settings settings);
}