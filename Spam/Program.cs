﻿using MailKit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spam;
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
    .AddSingleton(_ => new CommandLineArgumentsService(args))
    .AddSingleton(provider => configService.GetSettings());

using var serviceProvider = serviceCollection.BuildServiceProvider();

// Get the services
var spamProcessor = serviceProvider.GetRequiredService<ISpamProcessor>();
var imapClientFactory = serviceProvider.GetRequiredService<IImapClientFactory>();
var metricsService = serviceProvider.GetRequiredService<IMetricsService>();

var commandLineArgs = serviceProvider.GetRequiredService<CommandLineArgumentsService>();

using (var client = imapClientFactory.CreateImapClient())
{
    var inboxFolder = client.GetFolder("Inbox");
    var spamFolder = client.GetFolder(SpecialFolder.Junk);

    metricsService.SendReportEmails(commandLineArgs.ForceDailyReport, commandLineArgs.ForceMonthlyReport);

    if (commandLineArgs.ProcessSpam)
    {
        await spamProcessor.ProcessNewSpamMesssages(spamFolder);
        if(spamFolder.IsOpen)
        {
            await spamFolder.CloseAsync(true);
        }
    }

    if (commandLineArgs.ProcessResponses)
    {
        await spamProcessor.ProcessSpamCopResponses(inboxFolder);
        if (inboxFolder.IsOpen)
        {
            await inboxFolder.CloseAsync(true);
        }
    }

    client.Disconnect(true);
}

metricsService.SaveMetrics();
stopwatch.Stop();

Log.Information("Application Shutdown: Runtime {Runtime}", stopwatch.Elapsed);