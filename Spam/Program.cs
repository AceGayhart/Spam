using MailKit;
using Microsoft.Extensions.DependencyInjection;
using Spam;

var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfigurationService, ConfigurationService>()
    .AddSingleton<ISpamProcessor, SpamProcessor>()
    .AddSingleton<IImapClientFactory, ImapClientFactory>()
    .AddSingleton<ISmtpClientFactory, SmtpClientFactory>()
    .AddSingleton<IPuppeteerService, PuppeteerService>()
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

using (var client = imapClientFactory.CreateImapClient(settings))
{
    var trashFolder = client.GetFolder(SpecialFolder.Trash);
    var inboxFolder = client.GetFolder("Inbox");
    var spamFolder = client.GetFolder(SpecialFolder.Junk);

    await spamProcessor.ProcessNewSpamMesssages(spamFolder, trashFolder, settings, smtpClientFactory);
    await spamProcessor.ProcessSpamCopResponses(inboxFolder, trashFolder, settings, puppeteerService);

    client.Disconnect(true);
}