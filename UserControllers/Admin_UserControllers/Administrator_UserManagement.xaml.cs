using LibraryManagement.Models;
using LibraryManagement.ViewModels.Administrator_viewmodels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LibraryManagement.UserControllers.Admin_UserControllers
{
    public partial class Administrator_UserManagement : UserControl
    {
        public event Action BackToLibrariesRequested;

        private bool _isUpdatingPasswordBox;
        private INotifyPropertyChanged _currentNotifyContext;

        public Administrator_UserManagement()
            : this(null)
        {
        }

        public Administrator_UserManagement(Model_User currentAdmin)
        {
            InitializeComponent();

            DataContext = new AdminAccountsManagementViewModel(currentAdmin);

            DataContextChanged += Administrator_UserManagement_DataContextChanged;
            Loaded += Administrator_UserManagement_Loaded;
            Unloaded += Administrator_UserManagement_Unloaded;
        }

        private void BackToLibraries_Click(object sender, RoutedEventArgs e)
        {
            if (BackToLibrariesRequested != null)
            {
                BackToLibrariesRequested();
            }
        }

        private void Administrator_UserManagement_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeToViewModel();
            SyncPasswordBoxFromViewModel();
        }

        private void Administrator_UserManagement_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeFromViewModel();
        }

        private void Administrator_UserManagement_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeFromViewModel();
            SubscribeToViewModel();
            SyncPasswordBoxFromViewModel();
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPasswordBox)
            {
                return;
            }

            var viewModel = DataContext as AdminAccountsManagementViewModel;

            if (viewModel == null)
            {
                return;
            }

            viewModel.AccountPassword = passwordBox.Password;
        }

        private void SubscribeToViewModel()
        {
            _currentNotifyContext = DataContext as INotifyPropertyChanged;

            if (_currentNotifyContext != null)
            {
                _currentNotifyContext.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void UnsubscribeFromViewModel()
        {
            if (_currentNotifyContext != null)
            {
                _currentNotifyContext.PropertyChanged -= ViewModel_PropertyChanged;
                _currentNotifyContext = null;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AccountPassword")
            {
                SyncPasswordBoxFromViewModel();
            }
        }

        private void SyncPasswordBoxFromViewModel()
        {
            var viewModel = DataContext as AdminAccountsManagementViewModel;

            if (viewModel == null)
            {
                return;
            }

            var password = viewModel.AccountPassword;

            if (password == null)
            {
                password = string.Empty;
            }

            if (passwordBox.Password == password)
            {
                return;
            }

            _isUpdatingPasswordBox = true;
            passwordBox.Password = password;
            _isUpdatingPasswordBox = false;
        }
    }
}