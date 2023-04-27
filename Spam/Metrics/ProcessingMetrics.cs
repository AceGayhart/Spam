namespace Spam.Metrics;

public class ProcessingMetrics
{
    public List<MetricEntry> Entries { get; set; } = new List<MetricEntry>();
    public DateTime LastRunDate { get; set; } = DateTime.UtcNow;
}