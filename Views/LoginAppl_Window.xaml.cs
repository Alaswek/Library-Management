using LibraryManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LibraryManagement.Views
{
    public partial class LoginAppl_Window : Window
    {
        public LoginAppl_Window()
        {
            InitializeComponent();

            DataContext = new Login_ViewModel();
        }

        private void passBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            var viewModel = DataContext as Login_ViewModel;

            if (viewModel == null)
            {
                return;
            }

            viewModel.Password = passwordBox.Password;
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

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
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