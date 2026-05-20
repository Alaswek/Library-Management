using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using System;
using System.Windows;

namespace LibraryManagement.Views
{
    public partial class ChangePassword_Window : Window
    {
        public ChangePassword_Window() : this(null)
        {
        }

        public ChangePassword_Window(Model_User currentUser)
        {
            InitializeComponent();

            DataContext = new ChangePasswordViewModel(currentUser);
        }

        private void currentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ChangePasswordViewModel;

            if (viewModel != null)
            {
                viewModel.CurrentPassword = currentPasswordBox.Password;
            }
        }

        private void newPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ChangePasswordViewModel;

            if (viewModel != null)
            {
                viewModel.NewPassword = newPasswordBox.Password;
            }
        }

        private void confirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ChangePasswordViewModel;

            if (viewModel != null)
            {
                viewModel.ConfirmPassword = confirmPasswordBox.Password;
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ChangePasswordViewModel;

            if (viewModel == null)
            {
                return;
            }

            if (viewModel.ChangePasswordCommand.CanExecute(null))
            {
                viewModel.ChangePasswordCommand.Execute(null);
            }

            if (viewModel.PasswordChangedSuccessfully)
            {
                MessageBox.Show(
                    "Password changed successfully.",
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
    }
}