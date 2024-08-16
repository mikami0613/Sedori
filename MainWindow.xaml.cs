using System;
using System.Windows;
using System.Windows.Navigation;

namespace Sedori
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel MainViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel = new MainWindowViewModel();
            //XAML Viewで、MainViewModelのプロパティにアクセスするようになる
            DataContext = MainViewModel;
        }

        //ハイパーリンク開く
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            MainViewModel.OpenProductPage(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void ReserchButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainViewModel != null)
            {
                ReserchButton.IsEnabled = false;
                try
                {
                    await MainViewModel.ExeSerchAsync(SerchBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ReserchButton.IsEnabled = true;
                }
            }
        }
    }
}