using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Sedori
{
    public class LoginWindowViewModel : INotifyPropertyChanged
    {
        //このPropertyChangedという名前と、XAMLのPropertyChangedは名前は同じだけど、直接的な関係はない
        public event PropertyChangedEventHandler? PropertyChanged;

        //_を付けることで外部から書き換え不可にする。書き換えたい場合は、プロパティを経由することで安全にしている。
        private string _idText;
        public string IdText
        {
            get => _idText;
            set
            {
                // InputTextが永遠ループで変更されることを防ぐため
                if (_idText != value)
                {
                    _idText = value;
                    //プロパティが呼ばれたので、OnPropertyChanged
                    //プロパティ名を変更した場合、コンパイラがエラーを出すため、バグを防ぐことができます。
                    //OnPropertyChanged(nameof(InputText));
                    UpdateTextBoxBackground();
                }
            }
        }

        private SecureString _securePassword;

        public SecureString SecurePassword
        {
            get => _securePassword;
            set
            {
                if (value != _securePassword)
                {
                    _securePassword = value;
                    UpdatePassBoxBackground();

                }

            }
        }

        public void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxBackground();
        }

        public void PassBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePassBoxBackground();
        }

        private Brush _textBoxBackground = Brushes.White;
        public Brush TextBoxBackground
        {
            get => _textBoxBackground;
            set
            {
                _textBoxBackground = value;
                //TextBoxBackgroundの色を変えた時に、それをViewに伝える。
                //OnPropertyChanged(nameof(TextBoxBackground));
            }
        }

        private Brush _passBoxBackground = Brushes.White;
        public Brush PassBoxBackground
        {
            get => _passBoxBackground;
            set
            {
                _passBoxBackground = value;
                //TextBoxBackgroundの色を変えた時に、それをViewに伝える。
                //OnPropertyChanged(nameof(TextBoxBackground));
            }
        }

        //ログインIDのプロパティ
        public String LoginId{ get; set; }

        //ログインPassのプロパティ
        public String LoginPass { get; set; }

        private void UpdateTextBoxBackground()
        {
            //textBoxが編集されたら、
            TextBoxBackground = string.IsNullOrWhiteSpace(IdText)
                ? Brushes.Red : Brushes.White; //もし上の式が真なら、Redを、偽ならWhiteを返す。
            OnPropertyChanged(nameof(TextBoxBackground));
        }

        private void UpdatePassBoxBackground()
        {
            PassBoxBackground = IsSecureStringNullOrEmpty(SecurePassword)
                ? Brushes.Red : Brushes.White; //もし上の式が真なら、Redを、偽ならWhiteを返す。
            OnPropertyChanged(nameof(PassBoxBackground));
        }
        private bool IsSecureStringNullOrEmpty(SecureString secureString)
        {
            if (secureString == null)
                return true;

            return secureString.Length == 0;
        }

        protected virtual void OnPropertyChanged(string propertyName) 
        { 
            //これ意味ある？これしなかったらプロパティの変化がUIに反映されない？
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OpenMainPage()
        {

        }

        

    }
}
