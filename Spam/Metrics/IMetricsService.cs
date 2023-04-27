namespace Spam.Metrics;

public interface IMetricsService
{
    DateTime GetLastRunDateTime();

    void IncrementSpamCopReportFailures(int count = 1);

    void IncrementSpamCopReportsSent(int count = 1);

    void IncrementSpamCopResponsesRecieved(int count = 1);

    void IncrementSpamCopSubmissionsSent(int count = 1);

    void IncrementSpamEmailsRecieved(int count = 1);

    ProcessingMetrics LoadMetrics();

    void SaveMetrics();
}