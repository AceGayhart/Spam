using MimeKit;
using Spam.Configuration;
using Spam.Factories;
using System.Text;
using System.Text.Json;

namespace Spam.Metrics;

public class MetricsService : IMetricsService
{
    private readonly ProcessingMetrics _metrics;
    private readonly string _metricsFilePath;
    private readonly Settings _settings;
    private readonly ISmtpClientFactory _smtpClientFactory;
    private MetricEntry? _currentMetricEntry;

    public MetricsService(Settings settings, ISmtpClientFactory smtpClientFactory)
    {
        _settings = settings;
        _metricsFilePath = _settings.Metrics.FileStore;
        _smtpClientFactory = smtpClientFactory;

        _metrics = LoadMetrics();
    }

    public DateTime GetLastRunDateTime()
    {
        return _metrics.LastRunDate;
    }

    public IEnumerable<MetricEntry> GetMetrics(DateTime startDate, DateTime endDate)
    {
        return _metrics.Entries.Where(e => e.Timestamp >= startDate && e.Timestamp < endDate);
    }

    public void IncrementSpamCopReportFailures(int count = 1)
    {
        GetCurrentEntry().SpamCopReportFailures += count;
    }

    public void IncrementSpamCopReportsSent(int count = 1)
    {
        GetCurrentEntry().SpamCopReportsSent += count;
    }

    public void IncrementSpamCopResponsesRecieved(int count = 1)
    {
        GetCurrentEntry().SpamCopResponsesReceived += count;
    }

    public void IncrementSpamCopSubmissionsSent(int count = 1)
    {
        GetCurrentEntry().SpamCopSubmissionsSent += count;
    }

    public void IncrementSpamEmailsRecieved(int count = 1)
    {
        GetCurrentEntry().SpamEmailsReceived += count;
    }

    public ProcessingMetrics LoadMetrics()
    {
        if (File.Exists(_metricsFilePath))
        {
            var jsonString = File.ReadAllText(_metricsFilePath);
            return JsonSerializer.Deserialize<ProcessingMetrics>(jsonString) ?? new ProcessingMetrics();
        }

        return new ProcessingMetrics();
    }

    public void SaveMetrics()
    {
        if (_currentMetricEntry == null)
        {
            _metrics.LastRunDate = DateTime.UtcNow;
        }
        else
        {
            _metrics.LastRunDate = _currentMetricEntry.Timestamp;
            _metrics.Entries.Add(_currentMetricEntry);
            _currentMetricEntry = null;
        }

        var jsonString = JsonSerializer.Serialize(_metrics);
        File.WriteAllText(_metricsFilePath, jsonString);
    }

    public void SendReportEmails()
    {
        if (!_settings.Metrics.SendDailyMetricsReport && !_settings.Metrics.SendMonthlyMetricsReport)
        {
            return;
        }

        var reportDate = DateTime.Today.AddDays(-1);

        // Check for daily report
        if (DateTime.Now.Date != GetLastRunDateTime().ToLocalTime().Date)
        {
            using var smtpClient = _smtpClientFactory.CreateSmtpClient();
            if (_settings.Metrics.SendDailyMetricsReport)
            {
                var email = GenerateDailyReportEmail(reportDate);

                smtpClient.Send(email);
            }

            // Check for montly report
            if (DateTime.Now.Month != GetLastRunDateTime().ToLocalTime().Month
                && _settings.Metrics.SendMonthlyMetricsReport)
            {
                var email = GenerateMonthlyReportEmail(reportDate);
                smtpClient.Send(email);
            }

            smtpClient.Disconnect(true);
        }
    }

    private static string GenerateReport(IEnumerable<MetricEntry> metricEntries, bool summarizeByDate)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<style>");
        sb.AppendLine("table { border-collapse: collapse; font-family: Arial, sans-serif; }");
        sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; }");
        sb.AppendLine("th { background-color: #f2f2f2; }");
        sb.AppendLine("td.number { text-align: right; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Date</th><th>Spam</th><th>Submissions</th><th>Responses</th><th>Reports</th><th>Failures</th></tr>");

        if (summarizeByDate)
        {
            metricEntries = metricEntries
                .GroupBy(e => e.Timestamp.ToLocalTime().Date)
                .Select(g => new MetricEntry
                {
                    Timestamp = g.Key,
                    SpamCopReportFailures = g.Sum(e => e.SpamCopReportFailures),
                    SpamCopReportsSent = g.Sum(e => e.SpamCopReportsSent),
                    SpamCopResponsesReceived = g.Sum(e => e.SpamCopResponsesReceived),
                    SpamCopSubmissionsSent = g.Sum(e => e.SpamCopSubmissionsSent),
                    SpamEmailsReceived = g.Sum(e => e.SpamEmailsReceived),
                });
        }

        var totalEntry = new MetricEntry();
        int rowIndex = 0;

        void AppendCell(int value, bool showZero, bool red)
        {
            string cellStyle = red ? " style=\"color: red;\"" : "";
            sb.Append(value == 0 && !showZero
                ? $"<td class=\"number\"></td>"
                : $"<td class=\"number\"{cellStyle}>{value:#,##0}</td>");
        }

        foreach (var entry in metricEntries)
        {
            rowIndex++;
            string rowStyle = rowIndex % 2 == 0 ? " style=\"background-color: #eeeeee;\"" : "";
            if (summarizeByDate)
            {
                sb.Append($"<tr{rowStyle}><td>{entry.Timestamp.ToLocalTime():yyyy-MM-dd}</td>");
            }
            else
            {
                sb.Append($"<tr{rowStyle}><td>{entry.Timestamp.ToLocalTime():yyyy-MM-dd hh:mm:ss tt}</td>");
            }

            AppendCell(entry.SpamEmailsReceived, false, false);
            AppendCell(entry.SpamCopSubmissionsSent, false, false);
            AppendCell(entry.SpamCopResponsesReceived, false, false);
            AppendCell(entry.SpamCopReportsSent, false, false);
            AppendCell(entry.SpamCopReportFailures, false, true);

            sb.AppendLine("</tr>");

            totalEntry.SpamCopReportFailures += entry.SpamCopReportFailures;
            totalEntry.SpamCopReportsSent += entry.SpamCopReportsSent;
            totalEntry.SpamCopResponsesReceived += entry.SpamCopResponsesReceived;
            totalEntry.SpamCopSubmissionsSent += entry.SpamCopSubmissionsSent;
            totalEntry.SpamEmailsReceived += entry.SpamEmailsReceived;
        }

        sb.Append("<tr style=\"font-weight:bold; background-color: #dddddd;\"><td>Total</td>");
        AppendCell(totalEntry.SpamEmailsReceived, true, false);
        AppendCell(totalEntry.SpamCopSubmissionsSent, true, false);
        AppendCell(totalEntry.SpamCopResponsesReceived, true, false);
        AppendCell(totalEntry.SpamCopReportsSent, true, false);
        AppendCell(totalEntry.SpamCopReportFailures, true, true);
        sb.AppendLine("</tr>");

        sb.AppendLine("</table>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private MimeMessage GenerateDailyReportEmail(DateTime reportDate)
    {
        var startDate = reportDate.Date.ToUniversalTime();
        var endDate = startDate.AddDays(1);

        var summaryMetrics = GetMetrics(startDate, endDate);

        var htmlReport = GenerateReport(summaryMetrics, false);

        MailboxAddress address = new(_settings.MailServer.DisplayName, _settings.MailServer.EmailAddress);

        var email = new MimeMessage();
        email.From.Add(address);
        email.To.Add(address);

        email.Subject = $"Spam Metrics Daily Report: {startDate.ToLocalTime():yyyy-MM-dd}";

        email.Body = new TextPart("html") { Text = htmlReport };

        return email;
    }

    private MimeMessage GenerateMonthlyReportEmail(DateTime reportDate)
    {
        var startDate = reportDate.ToUniversalTime();
        startDate = new DateTime(startDate.Year, startDate.Month, 1).ToUniversalTime();
        var endDate = startDate.AddMonths(1);

        var summaryMetrics = GetMetrics(startDate, endDate);

        var htmlReport = GenerateReport(summaryMetrics, true);

        MailboxAddress address = new(_settings.MailServer.DisplayName, _settings.MailServer.EmailAddress);

        var email = new MimeMessage();
        email.From.Add(address);
        email.To.Add(address);

        email.Subject = $"Spam Metrics Monthly Report: {startDate:yyyy-MM}";

        email.Body = new TextPart("html") { Text = htmlReport };

        return email;
    }

    private MetricEntry GetCurrentEntry()
    {
        _currentMetricEntry ??= new MetricEntry();

        return _currentMetricEntry;
    }
}