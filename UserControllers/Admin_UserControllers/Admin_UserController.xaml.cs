using LibraryManagement.Models;
using LibraryManagement.ViewModels.Administrator_viewmodels;
using LibraryManagement.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LibraryManagement.UserControllers.Admin_UserControllers
{
    public partial class Admin_UserController : UserControl
    {
        public event Action OpenUserManagementRequested;
        public event Action<Model_Library> OpenLibraryBooksRequested;
        private readonly Model_User _currentAdmin;

        public Admin_UserController()
            : this(null)
        {
        }

        public Admin_UserController(Model_User currentAdmin)
        {
            InitializeComponent();
            _currentAdmin = currentAdmin;
            DataContext = new AdminLibraryManagementViewModel(currentAdmin);
        }

        private void OpenUserManagement_Click(object sender, RoutedEventArgs e)
        {
            if (OpenUserManagementRequested != null)
            {
                OpenUserManagementRequested();
            }
        }

        private void ManageBooks_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button == null)
            {
                return;
            }

            var library = button.CommandParameter as Model_Library;

            if (library == null)
            {
                MessageBox.Show(
                    "Could not identify the selected library.",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (OpenLibraryBooksRequested != null)
            {
                OpenLibraryBooksRequested(library);
            }
        }

    }
}