using MailKit;
using Microsoft.Extensions.DependencyInjection;
using Spam.Configuration;
using Spam.Factories;
using Spam.Metrics;
using Spam.Puppet;
using Spam.SpamProcessing;

var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfigurationService, ConfigurationService>()
    .AddSingleton<ISpamProcessor, SpamProcessor>()
    .AddSingleton<IImapClientFactory, ImapClientFactory>()
    .AddSingleton<ISmtpClientFactory, SmtpClientFactory>()
    .AddSingleton<IPuppeteerService, PuppeteerService>()
    .AddSingleton<IMetricsService, MetricsService>()
    .BuildServiceProvider();

// Get configuration and settings
var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
configurationService.ConfigureLogger();
var settings = configurationService.GetSettings();

// Get the services
var spamProcessor = serviceProvider.GetRequiredService<ISpamProcessor>();
var imapClientFactory = serviceProvider.GetRequiredService<IImapClientFactory>();
var smtpClientFactory = serviceProvider.GetRequiredService<ISmtpClientFactory>();
var puppeteerService = serviceProvider.GetRequiredService<IPuppeteerService>();
var metricsService = serviceProvider.GetRequiredService<IMetricsService>();

using (var client = imapClientFactory.CreateImapClient(settings))
{
    var trashFolder = client.GetFolder(SpecialFolder.Trash);
    var inboxFolder = client.GetFolder("Inbox");
    var spamFolder = client.GetFolder(SpecialFolder.Junk);

    metricsService.SendReportEmails(settings, smtpClientFactory);

    await spamProcessor.ProcessNewSpamMesssages(spamFolder, trashFolder, settings, smtpClientFactory, metricsService);
    await spamProcessor.ProcessSpamCopResponses(inboxFolder, trashFolder, settings, puppeteerService, metricsService);

    client.Disconnect(true);
}

metricsService.SaveMetrics();