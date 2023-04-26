namespace Spam;

public interface IPuppeteerService
{
    Task<List<string>> ProcessHtmlAndSendPostRequest(string url);
}