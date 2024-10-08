﻿using System.Diagnostics;
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

        private void PassBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_loginWindowViewModel != null)
            {
                _loginWindowViewModel.PassBox_GotFocus(sender, e);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loginWindowViewModel != null)
            {
                //パスワードとユーザーIDが合っていたら、の処理を入れる。
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _loginWindowViewModel.SecurePassword = ((PasswordBox)sender).SecurePassword;
 
        }
    }
}