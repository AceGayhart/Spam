namespace Spam.Configuration;

public class MetricsSettings
{
    public string FileStore { get; set; } = "email_metrics.json";
    public bool SendDailyMetricsReport { get; set; } = true;
    public bool SendMonthlyMetricsReport { get; set; } = true;
}