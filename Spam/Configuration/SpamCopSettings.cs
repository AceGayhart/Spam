namespace Spam.Configuration;

public class SpamCopSettings
{
    public int MaxAttachmentsPerReport { get; set; } = 25;
    public string ReportDisplayName { get; set; } = null!;
    public string ReportEmailAddress { get; set; } = null!;
    public string ReportUrl { get; set; } = null!;
    public string ResponseEmailAddress { get; set; } = null!;
    public string ResponseSubject { get; set; } = null!;
}