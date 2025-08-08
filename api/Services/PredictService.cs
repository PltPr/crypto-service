using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using Microsoft.Playwright;

namespace api.Services
{
    public class PredictService : IPredictService
    {
        public async Task<string?> GetStatusForCrypto(string? symbol = "BTCUSDT", string? interval = "1 hour")
        {
            symbol ??= "BTCUSDT";
            interval ??= "1 hour";


            using var playwright = await Playwright.CreateAsync();

            var url = $"https://www.tradingview.com/symbols/{symbol}/technicals";

            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync(url);

            

            await page.WaitForSelectorAsync($"button:has-text('{interval}')");
            await page.ClickAsync($"button:has-text('{interval}')");

            await Task.Delay(3000);

            var summaryWrapper = await page.QuerySelectorAsync("div[class*='summary-']");

            if (summaryWrapper == null)
            {
                Console.WriteLine("Nie znaleziono sekcji Summary");
                return null;
            }


            var container = await summaryWrapper.QuerySelectorAsync("div[class*='container-vLbFM67a']");

            if (container == null)
            {
                Console.WriteLine("Nie znaleziono kontenera z ratingiem w Summary");
                return null;
            }

            var classAttr = await container.GetAttributeAsync("class");

            Console.WriteLine(classAttr);

            string rating = classAttr switch
            {
                var s when s.Contains("strong-sell") => "Strong Sell",
                var s when s.Contains("strong-buy") => "Strong Buy",
                var s when s.Contains("sell") => "Sell",
                var s when s.Contains("neutral") => "Neutral",
                var s when s.Contains("buy") => "Buy",
                _ => "Unknown"
            };

            await page.CloseAsync();

            return rating;
        }
    }
}