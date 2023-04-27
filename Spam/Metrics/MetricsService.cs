using System.Text.Json;

namespace Spam.Metrics;

public class MetricsService : IMetricsService
{
    private const string MetricsFilePath = "email_metrics.json";

    private readonly ProcessingMetrics _metrics;

    private MetricEntry? _currentMetricEntry;

    public MetricsService()
    {
        _metrics = LoadMetrics();
    }

    public DateTime GetLastRunDateTime()
    {
        return _metrics.LastRunDate;
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
        if (File.Exists(MetricsFilePath))
        {
            var jsonString = File.ReadAllText(MetricsFilePath);
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
        File.WriteAllText(MetricsFilePath, jsonString);
    }

    private MetricEntry GetCurrentEntry()
    {
        _currentMetricEntry ??= new MetricEntry();

        return _currentMetricEntry;
    }
}