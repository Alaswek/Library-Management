using LibraryManagement.ViewModels;
using System;
using System.Windows;
using LibraryManagement.UserControllers.Admin_UserControllers;

namespace LibraryManagement.Views
{
    public partial class ResetPassword_Window : Window
    {
        public ResetPassword_Window()
        {
            InitializeComponent();

            DataContext = new ResetPasswordViewModel();
        }

        private void newPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ResetPasswordViewModel;

            if (viewModel != null)
            {
                viewModel.NewPassword = newPasswordBox.Password;
            }
        }

        private void confirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ResetPasswordViewModel;

            if (viewModel != null)
            {
                viewModel.ConfirmPassword = confirmPasswordBox.Password;
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ResetPasswordViewModel;

            if (viewModel == null)
            {
                return;
            }

            if (viewModel.ResetPasswordCommand.CanExecute(null))
            {
                viewModel.ResetPasswordCommand.Execute(null);
            }

            if (viewModel.PasswordResetSuccessfully)
            {
                MessageBox.Show(
                    "Password was reset successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CloseWithResult(true);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWithResult(false);
        }

        private void CloseWithResult(bool result)
        {
            try
            {
                DialogResult = result;
            }
            catch (InvalidOperationException)
            {
                Close();
            }
        }

        private void ResetAdmin_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(new AminRecoveryPassword_UserController());
        }
    }
}