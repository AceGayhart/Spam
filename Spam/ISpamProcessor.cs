using MailKit;

namespace Spam;

public interface ISpamProcessor
{
    Task ProcessNewSpamMesssages(
        IMailFolder spamFolder,
        IMailFolder trashFolder,
        Settings settings,
        ISmtpClientFactory smtpClientFactory);

    Task ProcessSpamCopResponses(
        IMailFolder inboxFolder,
        IMailFolder trashFolder,
        Settings settings,
        IPuppeteerService puppeteerService);
}