using MailKit;

namespace Spam.SpamProcessing;

public interface ISpamProcessor
{
    Task ProcessNewSpamMesssages(IMailFolder spamFolder, IMailFolder trashFolder);

    Task ProcessSpamCopResponses(IMailFolder inboxFolder, IMailFolder trashFolder);
}