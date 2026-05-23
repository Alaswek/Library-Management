using LibraryManagement.Models;
using LibraryManagement.ViewModels.Administrator_viewmodels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LibraryManagement.UserControllers.Admin_UserControllers
{
    public partial class AdminLibraryBooks_UserController : UserControl
    {
        public event Action BackToLibrariesRequested;

        private readonly Model_User _currentAdmin;
        private readonly Model_Library _selectedLibrary;
        private readonly AdminLibraryBooksViewModel _viewModel;

        public AdminLibraryBooks_UserController()
            : this(null, null)
        {
        }

        public AdminLibraryBooks_UserController(Model_User currentAdmin, Model_Library selectedLibrary)
        {
            InitializeComponent();

            _currentAdmin = currentAdmin;
            _selectedLibrary = selectedLibrary;

            _viewModel = new AdminLibraryBooksViewModel(currentAdmin, selectedLibrary);
            DataContext = _viewModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDataFromViewModel();
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                RefreshDataFromViewModel();
            }
        }

        private void BackToLibraries_Click(object sender, RoutedEventArgs e)
        {
            if (BackToLibrariesRequested != null)
            {
                BackToLibrariesRequested();
            }
        }

        private void RefreshDataFromViewModel()
        {
            if (_viewModel == null)
            {
                return;
            }

            if (_viewModel.RefreshDataCommand != null &&
                _viewModel.RefreshDataCommand.CanExecute(null))
            {
                _viewModel.RefreshDataCommand.Execute(null);
            }
        }
    }
}