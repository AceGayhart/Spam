using MailKit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spam.Configuration;
using Spam.Factories;
using Spam.Metrics;
using Spam.Puppet;
using Spam.SpamProcessing;
using System.Diagnostics;
using System.Reflection;

var serviceCollection = new ServiceCollection()
    .AddSingleton<IConfigurationService, ConfigurationService>();

using var tempServiceProvider = serviceCollection.BuildServiceProvider();
var configService = tempServiceProvider.GetRequiredService<IConfigurationService>();
configService.ConfigureLogger();

var stopwatch = Stopwatch.StartNew();
var assembly = Assembly.GetExecutingAssembly();
var appName = assembly.GetName().Name;
var version = assembly.GetName().Version;
var buildDate = File.GetLastWriteTime(assembly.Location);

Log.Information("{AppName} Startup: Version {Version}, Built on {BuildDate}", appName, version, buildDate);

// Register the rest of the services
serviceCollection
    .AddSingleton<ISpamProcessor, SpamProcessor>()
    .AddSingleton<IImapClientFactory, ImapClientFactory>()
    .AddSingleton<ISmtpClientFactory, SmtpClientFactory>()
    .AddSingleton<IPuppeteerService, PuppeteerService>()
    .AddSingleton<IMetricsService, MetricsService>()
    .AddSingleton(provider => configService.GetSettings());

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
stopwatch.Stop();

Log.Information("Application Shutdown: Runtime {Runtime}", stopwatch.Elapsed);