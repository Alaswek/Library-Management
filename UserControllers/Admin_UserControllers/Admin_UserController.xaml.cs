using LibraryManagement.Models;
using LibraryManagement.ViewModels.Administrator_viewmodels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LibraryManagement.UserControllers.Admin_UserControllers
{
    public partial class Admin_UserController : UserControl
    {
        public event Action OpenUserManagementRequested;

        public Admin_UserController()
            : this(null)
        {
        }

        public Admin_UserController(Model_User currentAdmin)
        {
            InitializeComponent();

            DataContext = new AdminLibraryManagementViewModel(currentAdmin);
        }

        private void OpenUserManagement_Click(object sender, RoutedEventArgs e)
        {
            if (OpenUserManagementRequested != null)
            {
                OpenUserManagementRequested();
            }
        }
    }
}