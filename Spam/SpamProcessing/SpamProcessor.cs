using MailKit;
using MailKit.Search;
using MimeKit;
using Serilog;
using Spam.Configuration;
using Spam.Factories;
using Spam.Metrics;
using Spam.Puppet;
using System.Text.RegularExpressions;

namespace Spam.SpamProcessing;

public partial class SpamProcessor : ISpamProcessor
{
    private static readonly ILogger Log = Serilog.Log.ForContext<SpamProcessor>();

    public async Task ProcessNewSpamMesssages(
                IMailFolder spamFolder,
            IMailFolder trashFolder,
            Settings settings,
            ISmtpClientFactory smtpClientFactory,
            IMetricsService metricsService)
    {
        Log.Information("Checking for new spam");
        await spamFolder.OpenAsync(FolderAccess.ReadWrite);

        var query = SearchQuery.All;

        var messageIds = await spamFolder.SearchAsync(query);

        if (messageIds.Count == 0)
        {
            Log.Information("No new spam emails");
            return;
        }

        Log.Information("Found new spam emails: {count}", messageIds.Count);

        List<MimeMessage> messages = new();

        foreach (var uid in messageIds)
        {
            var message = spamFolder.GetMessage(uid);

            if (IsSenderAllowed(message.From, settings.AllowedSenders))
            {
                Log.Information("Skipping email from {from}: {subject}", message.From.FirstOrDefault(), message.Subject);
                spamFolder.RemoveFlags(uid, MessageFlags.Seen, true);
                continue;
            }

            messages.Add(message);
            metricsService.IncrementSpamEmailsRecieved();

            spamFolder.AddFlags(uid, MessageFlags.Seen, true);
            spamFolder.MoveTo(uid, trashFolder);
        }

        MailboxAddress fromAddress = new(settings.MailServer.DisplayName, settings.MailServer.EmailAddress);
        MailboxAddress toAddress = new(settings.SpamCop.ReportDisplayName, settings.SpamCop.ReportEmailAddress);

        var spamCopSubmission = GenerateSpamCopSubmission(messages, fromAddress, toAddress);

        using (var client = smtpClientFactory.CreateSmtpClient(settings))
        {
            client.Send(spamCopSubmission);
            metricsService.IncrementSpamCopSubmissionsSent();
            client.Disconnect(true);
        };
    }

    public async Task ProcessSpamCopResponses(
                IMailFolder inboxFolder,
                IMailFolder trashFolder,
                Settings settings,
                IPuppeteerService puppeteerService,
                IMetricsService metricsService)
    {
        Log.Information("Checking for SpamCop responses");
        await inboxFolder.OpenAsync(FolderAccess.ReadWrite);

        var query = SearchQuery.FromContains(settings.SpamCop.ResponseEmailAddress)
            .And(SearchQuery.SubjectContains(settings.SpamCop.ResponseSubject));

        var messageIds = await inboxFolder.SearchAsync(query);

        if (messageIds.Count == 0)
        {
            Log.Information("No new SpamCop responses");
            return;
        }

        Log.Information("Found new SpamCop emails: {count}", messageIds.Count);
        metricsService.IncrementSpamCopResponsesRecieved();

        List<MimeMessage> messages = new();

        foreach (var uid in messageIds)
        {
            messages.Add(inboxFolder.GetMessage(uid));
            var msg = inboxFolder.GetMessage(uid);

            var links = ExtractLinks(msg.TextBody, settings.SpamCop.ReportUrl);
            foreach (var link in links)
            {
                Log.Debug("Processing link: {link}", link);

                try
                {
                    var postResult = await puppeteerService.ProcessHtmlAndSendPostRequest(link);

                    foreach (var post in postResult)
                    {
                        Log.Debug(post);
                    }

                    metricsService.IncrementSpamCopReportsSent();
                }
                catch (Exception ex)
                {
                    metricsService.IncrementSpamCopReportFailures();
                    Log.Fatal(ex, "Failed to send SpamCop report");
                }
            }

            inboxFolder.AddFlags(uid, MessageFlags.Seen, true);
            inboxFolder.MoveTo(uid, trashFolder);
        }
    }

    private static string[] ExtractLinks(string input, string urlPrefix)
    {
        var regex = UrlRegex();
        var matches = regex.Matches(input);

        return matches
            .Select(match => match.Value)
            .Where(link => link.StartsWith(urlPrefix))
            .ToArray();
    }

    private static MimeMessage GenerateSpamCopSubmission(List<MimeMessage> messages, MailboxAddress fromAddress, MailboxAddress toAddress)
    {
        var submission = new MimeMessage();
        submission.From.Add(fromAddress);
        submission.To.Add(toAddress);

        submission.Subject = $"Forwarded Spam: {messages.Count} messages";

        var multipart = new Multipart("mixed")
        {
            new TextPart("plain") { Text = "These are the forwarded spam emails."}
        };

        foreach (var message in messages)
        {
            var messagePart = new MessagePart
            {
                Message = message,
                IsAttachment = true
            };

            multipart.Add(messagePart);
        }

        submission.Body = multipart;
        return submission;
    }

    private static bool IsSenderAllowed(InternetAddressList senderList, List<string>? allowedSenders)
    {
        if (senderList == null || allowedSenders == null || allowedSenders.Count == 0)
        {
            return true;
        }

        return senderList.OfType<MailboxAddress>()
            .Any(mailbox =>
                allowedSenders.Any(allowed =>
                    allowed.Equals(mailbox.Address, StringComparison.OrdinalIgnoreCase) ||
                    allowed.Equals(mailbox.Domain, StringComparison.OrdinalIgnoreCase)));
    }

    [GeneratedRegex("https?://\\S+")]
    private static partial Regex UrlRegex();
}