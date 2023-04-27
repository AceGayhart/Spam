using PuppeteerSharp;
using Serilog;
using System.Text.RegularExpressions;

namespace Spam.Puppet;

public partial class PuppeteerService : IPuppeteerService
{
    public async Task<List<string>> ProcessHtmlAndSendPostRequest(string url)
    {
        // Set up PuppeteerSharp
        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        var options = new LaunchOptions
        {
            Headless = true,
            Timeout = 60 * 1000
        };
        using var browser = await Puppeteer.LaunchAsync(options);

        // Load the HTML content into a new page
        using var page = await browser.NewPageAsync();
        await page.GoToAsync(url);

        // Find and click the "Send" button
        var sendButton = await page.XPathAsync("//input[@type='submit' and @value='Send Spam Report(s) Now']");

        // The Send button could be missing if the report has already been submitted, if the spam is too
        // old to report, or if the ISP has already indicated that they will be stopping the spam. I.e.,
        // a lack of a Send button isn't necessarily a failure.
        if (sendButton.Length == 0)
        {
            var errorMessage = await page.QuerySelectorAsync(".error");
            if (errorMessage != null)
            {
                string errorContent = await errorMessage.EvaluateFunctionAsync<string>("em => em.textContent");
                return new List<string>() { errorContent };
            }

            var content = await page.GetContentAsync();
            string searchString = "<div class=\"header\">If reported today, reports would be sent to:</div>";
            var headerIndex = content.IndexOf(searchString);

            // If the header text was found, extract the text under the header
            if (headerIndex >= 0)
            {
                var startIndex = headerIndex + searchString.Length;
                var endIndex = content.IndexOf("</div>", startIndex);
                var text = content[startIndex..endIndex];

                string noHtml = StripHtmlTagsRegex().Replace(text.Replace("<br><br>", " "), "");
                List<string> lines = noHtml.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                lines.Insert(0, "The spam already generated the following report(s):");
                return lines;
            }

            Log.Error("Neither a Send button or error message was found");

            return new List<string>() { "Send button not found" };
        }

        var navigationTask = page.WaitForNavigationAsync();
        await sendButton[0].ClickAsync();
        await navigationTask;

        // Check if the navigation was successful
        var response = await navigationTask;
        if (response != null && response.Ok)
        {
            // Extract the content of an element with the class 'response-message'
            var messageElement = await page.QuerySelectorAsync("#content");
            if (messageElement != null)
            {
                string messageContent = await messageElement.EvaluateFunctionAsync<string>("el => el.textContent");

                var contentList = messageContent.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                return contentList;
            }
            else
            {
                return new List<string>() { "Message element not found" };
            }
        }
        else
        {
            return new List<string>() { "Failure" };
        }
    }

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex StripHtmlTagsRegex();
}