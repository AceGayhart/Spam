namespace Spam.Puppet;

public interface IPuppeteerService
{
    Task<List<string>> ProcessHtmlAndSendPostRequest(string url);
}