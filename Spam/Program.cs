using MailKit;
using Microsoft.Extensions.DependencyInjection;
using Spam.Configuration;
using Spam.Factories;
using Spam.Metrics;
using Spam.Puppet;
using Spam.SpamProcessing;

var serviceCollection = new ServiceCollection()
    .AddSingleton<IConfigurationService, ConfigurationService>()
    .AddSingleton<ISpamProcessor, SpamProcessor>()
    .AddSingleton<IImapClientFactory, ImapClientFactory>()
    .AddSingleton<ISmtpClientFactory, SmtpClientFactory>()
    .AddSingleton<IPuppeteerService, PuppeteerService>()
    .AddSingleton<IMetricsService, MetricsService>()
    .AddSingleton(provider =>
    {
        var configService = provider.GetRequiredService<IConfigurationService>();
        configService.ConfigureLogger();
        return configService.GetSettings();
    });

using var serviceProvider = serviceCollection.BuildServiceProvider();

// Get the services
var spamProcessor = serviceProvider.GetRequiredService<ISpamProcessor>();
var imapClientFactory = serviceProvider.GetRequiredService<IImapClientFactory>();
var metricsService = serviceProvider.GetRequiredService<IMetricsService>();

using (var client = imapClientFactory.CreateImapClient())
{
    var trashFolder = client.GetFolder(SpecialFolder.Trash);
    var inboxFolder = client.GetFolder("Inbox");
    var spamFolder = client.GetFolder(SpecialFolder.Junk);

    metricsService.SendReportEmails();

    await spamProcessor.ProcessNewSpamMesssages(spamFolder, trashFolder);
    await spamProcessor.ProcessSpamCopResponses(inboxFolder, trashFolder);

    client.Disconnect(true);
}

metricsService.SaveMetrics();