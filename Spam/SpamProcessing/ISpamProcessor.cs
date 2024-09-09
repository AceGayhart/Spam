using MailKit;

namespace Spam.SpamProcessing;

public interface ISpamProcessor
{
    Task ProcessNewSpamMesssages(IMailFolder spamFolder);

    Task ProcessSpamCopResponses(IMailFolder inboxFolder);
}