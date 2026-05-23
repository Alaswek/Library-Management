using LibraryManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LibraryManagement.Views
{
    public partial class LoginAppl_Window : Window
    {
        private bool _isPasswordSyncing = false;

        public LoginAppl_Window()
        {
            InitializeComponent();

            DataContext = new Login_ViewModel();
        }

        private void passBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isPasswordSyncing)
            {
                return;
            }

            var passwordBox = sender as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            _isPasswordSyncing = true;

            if (visiblePassBox != null)
            {
                visiblePassBox.Text = passwordBox.Password;
            }

            UpdateViewModelPassword(passwordBox.Password);

            _isPasswordSyncing = false;
        }

        private void visiblePassBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPasswordSyncing)
            {
                return;
            }

            var textBox = sender as TextBox;

            if (textBox == null)
            {
                return;
            }

            _isPasswordSyncing = true;

            if (passBox != null)
            {
                passBox.Password = textBox.Text;
            }

            UpdateViewModelPassword(textBox.Text);

            _isPasswordSyncing = false;
        }

        private void showPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _isPasswordSyncing = true;

            visiblePassBox.Text = passBox.Password;

            passBox.Visibility = Visibility.Collapsed;
            visiblePassBox.Visibility = Visibility.Visible;

            visiblePassBox.Focus();
            visiblePassBox.CaretIndex = visiblePassBox.Text.Length;

            UpdateViewModelPassword(visiblePassBox.Text);

            _isPasswordSyncing = false;
        }

        private void showPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _isPasswordSyncing = true;

            passBox.Password = visiblePassBox.Text;

            visiblePassBox.Visibility = Visibility.Collapsed;
            passBox.Visibility = Visibility.Visible;

            passBox.Focus();

            UpdateViewModelPassword(passBox.Password);

            _isPasswordSyncing = false;
        }

        private void UpdateViewModelPassword(string password)
        {
            var viewModel = DataContext as Login_ViewModel;

            if (viewModel == null)
            {
                return;
            }

            viewModel.Password = password;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResetPasswordWithCode_Click(object sender, RoutedEventArgs e)
        {
            var window = new ResetPassword_Window();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}