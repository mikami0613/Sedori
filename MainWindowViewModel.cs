using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

namespace Sedori
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private RakutenService _rakutenService;
        private Dispatcher _dispatcher;

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                Debug.WriteLine($"Products set: {_products?.Count} items");
                OnPropertyChanged(nameof(Products));
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(Status));
                });
            }
        }

        public MainWindowViewModel()
        {
            Products = new ObservableCollection<Product>();
            _rakutenService = new RakutenService("", "");
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OpenProductPage(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        public async Task ExeSerchAsync(string keyword)
        {
            try
            {
                Status = "検索中...";
                Debug.WriteLine($"Searching for: {keyword}");
                var products = await _rakutenService.SearchAsync(keyword);
                Debug.WriteLine($"Search completed: {products.Count} items found");
                await _dispatcher.InvokeAsync(() =>
                {
                    if (Products == null)
                    {
                        Products = new ObservableCollection<Product>(products);
                    }
                    else
                    {
                        Products.Clear();
                        foreach (var product in products)
                        {
                            Products.Add(product);
                        }
                    }
                    OnPropertyChanged(nameof(Products));
                    Debug.WriteLine($"Products updated in UI thread: {Products.Count} items");
                });

                await UpdatePointsAsync();
                Status = "完了";
            }
            catch (Exception ex)
            {
                Status = $"エラー: {ex.Message}";
                Debug.WriteLine($"検索エラー: {ex}");
            }
        }

        private async Task UpdatePointsAsync()
        {
            using var semaphore = new SemaphoreSlim(5);
            var tasks = Products.Select(async (product, index) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await _dispatcher.InvokeAsync(() => Status = $"ポイント取得中 ({index + 1}/{Products.Count})");
                    var points = await _rakutenService.GetRakutenPointsAsync(product.Url);
                    if (points != null)
                    {
                        await _dispatcher.InvokeAsync(() =>
                        {
                            product.Points = int.Parse(points);
                            OnPropertyChanged($"Products[{Products.IndexOf(product)}]");
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ポイント取得エラー: {ex}");
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();
            await Task.WhenAll(tasks);
        }

        public void Dispose()
        {
            _rakutenService?.Dispose();
        }
    }
}