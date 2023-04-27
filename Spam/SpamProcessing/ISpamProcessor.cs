using MailKit;
using Spam.Configuration;
using Spam.Factories;
using Spam.Metrics;
using Spam.Puppet;

namespace Spam.SpamProcessing;

public interface ISpamProcessor
{
    Task ProcessNewSpamMesssages(
        IMailFolder spamFolder,
        IMailFolder trashFolder,
        Settings settings,
        ISmtpClientFactory smtpClientFactory,
        IMetricsService metricsService);

    Task ProcessSpamCopResponses(
        IMailFolder inboxFolder,
        IMailFolder trashFolder,
        Settings settings,
        IPuppeteerService puppeteerService,
        IMetricsService metricsService);
}