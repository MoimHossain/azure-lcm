

using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AzLcm.Shared.PageScrapping
{
    public class HtmlExtractor(ILogger<HtmlExtractor> logger)
    {
        private IPlaywright? playwright;

        public async Task PrepareAsync()
        {
            //logger.Log(LogLevel.Information, "Creating playwright instances..");

            //try
            //{
            //    playwright = await Playwright.CreateAsync();
            //    await playwright.Chromium.LaunchAsync();
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}
        }

        public void TearDownAsync()
        {
            //playwright?.Dispose();
        }

        public async Task ExtractAsync(Uri hRef)
        {
            //using var playwright = await Playwright.CreateAsync();
            //var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            //{
            //    Headless = true
            //});
            //if(playwright != null )
            //{
            //    var browser = await playwright.Chromium.LaunchAsync();
            //    var page = await browser.NewPageAsync();

            //    await page.GotoAsync(hRef.ToString());
            //    await page.CloseAsync();
            //}
        }
    }
}
