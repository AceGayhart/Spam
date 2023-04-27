namespace Spam.Metrics;

public class MetricEntry
{
    public int SpamCopReportFailures { get; set; }
    public int SpamCopReportsSent { get; set; }
    public int SpamCopResponsesReceived { get; set; }
    public int SpamCopSubmissionsSent { get; set; }
    public int SpamEmailsReceived { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}