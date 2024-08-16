using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Diagnostics;

namespace Sedori
{
    public class RakutenService : IDisposable
    {
        private static readonly HttpClient client = new HttpClient();
        private const string API_ENDPOINT = "https://app.rakuten.co.jp/services/api/IchibaItem/Search/20170706";
        private const string APPLICATION_ID = "1061927745372814395";
        private const int DefaultTimeout = 120;

        private readonly string _username;
        private readonly string _password;
        private IWebDriver _driver;
        private bool _isLoggedIn = false;

        public RakutenService(string username, string password)
        {
            _username = username;
            _password = password;
            var options = new ChromeOptions();
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(DefaultTimeout);
        }

        public async Task<ObservableCollection<Product>> SearchAsync(string keyword)
        {
            var parameters = new Dictionary<string, string>
            {
                {"applicationId", APPLICATION_ID},
                {"keyword", keyword},
                {"format", "xml"},
                {"sort", "standard"},
                {"hits", "30"}
            };

            var url = $"{API_ENDPOINT}?{string.Join("&", parameters.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))}";
            try
            {
                var response = await client.GetStringAsync(url);
                var xdoc = XDocument.Parse(response);

                var products = xdoc.Descendants("Item")
                    .Select(item => new Product
                    {
                        Name = item.Element("itemName")?.Value,
                        Url = item.Element("itemUrl")?.Value,
                        Description = item.Element("itemCaption")?.Value,
                        ImageUrl = item.Element("mediumImageUrls")?.Element("imageUrl")?.Value,
                        Price = int.Parse(item.Element("itemPrice")?.Value ?? "0"),
                        Points = 0
                    })
                    .ToList();

                Debug.WriteLine($"API returned {products.Count} products");
                return new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API検索エラー: {ex.Message}");
                return new ObservableCollection<Product>();
            }
        }

        public async Task<string> GetRakutenPointsAsync(string url)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                if (!_isLoggedIn)
                {
                    await LoginAsync(cts.Token);
                }
                await NavigateToProductPageAsync(url, cts.Token);
                return await ExtractPointsAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("ポイント取得がタイムアウトしました。");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ポイント取得中にエラーが発生しました: {ex.Message}");
                return null;
            }
        }

        private async Task LoginAsync(CancellationToken cancellationToken = default)
        {
            if (!_isLoggedIn)
            {
                try
                {
                    _driver.Navigate().GoToUrl("https://grp02.id.rakuten.co.jp/rms/nid/vc?__event=login&service_id=top");
                    await WaitForPageLoad(cancellationToken);

                    _driver.FindElement(By.Id("loginInner_u")).SendKeys(_username);
                    _driver.FindElement(By.Id("loginInner_p")).SendKeys(_password);
                    _driver.FindElement(By.Name("submit")).Click();

                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(DefaultTimeout));
                    wait.Until(d => d.Url.Contains("www.rakuten.co.jp"));

                    _isLoggedIn = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ログインエラー: {ex.Message}");
                    throw;
                }
            }
        }

        private async Task NavigateToProductPageAsync(string url, CancellationToken cancellationToken = default)
        {
            _driver.Navigate().GoToUrl(url);
            await WaitForPageLoad(cancellationToken);
        }

        private async Task<string> ExtractPointsAsync(CancellationToken cancellationToken = default)
        {
            await WaitForPageLoad(cancellationToken);

            string script = @"
                var elements = document.getElementsByTagName('*');
                for (var i = 0; i < elements.length; i++) {
                    var element = elements[i];
                    if (element.textContent.includes('ポイント')) {
                        var pointMatch = element.textContent.match(/(\d{1,3}(,\d{3})*)\s*ポイント/);
                        if (pointMatch) {
                            return pointMatch[1];
                        }
                    }
                }
                return null;
            ";

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(DefaultTimeout));
            var result = wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript(script));

            if (result != null)
            {
                string pointText = result.ToString();
                return pointText.Replace(",", "");
            }

            Console.WriteLine("ポイント情報が見つかりませんでした。");
            return null;
        }

        private async Task WaitForPageLoad(CancellationToken cancellationToken = default)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(DefaultTimeout));
            await Task.Run(() => wait.Until(d =>
                ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete") &&
                d.FindElements(By.TagName("body")).Count > 0
            ), cancellationToken);
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}