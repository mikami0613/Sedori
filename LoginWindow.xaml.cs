using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sedori
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        LoginWindowViewModel _loginWindowViewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _loginWindowViewModel = new LoginWindowViewModel();
            //これによって、XAMLがLoginWindowViewModelでプロパティを発見できるようになるらしい。
            DataContext = _loginWindowViewModel;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_loginWindowViewModel != null)
            {
                _loginWindowViewModel.TextBox_GotFocus(sender, e);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("デバッグ文を表示");
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}