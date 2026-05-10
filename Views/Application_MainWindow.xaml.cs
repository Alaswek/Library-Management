using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.UserControllers.Admin_UserControllers;
using LibraryManagement.UserControllers.Librarian_UserControllers;
using System.ComponentModel;
using System.Windows;

namespace LibraryManagement.Views
{
    public partial class Application_MainWindow : Window
    {
        private Model_User _currentUser;

        public Application_MainWindow(Model_User user)
        {
            InitializeComponent();

            _currentUser = user;

            string role = string.Empty;

            if (user != null && user.Role != null)
            {
                role = user.Role.Trim().ToLowerInvariant();
            }

            if (role == "administrator" || role == "admin")
            {
                ShowAdminLibraryManagement();
            }
            else if (role == "librarian")
            {
                MainContentArea.Content = new Librarian_UserController(user);
            }
            else
            {
                string roleText = string.Empty;

                if (user != null)
                {
                    roleText = user.Role;
                }

                MessageBox.Show(
                    "Unknown user role: " + roleText,
                    "Login Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
            }
        }

        private void ShowAdminLibraryManagement()
        {
            var libraryView = new Admin_UserController(_currentUser);

            libraryView.OpenUserManagementRequested += ShowAdminUserManagement;

            MainContentArea.Content = libraryView;
        }

        private void ShowAdminUserManagement()
        {
            var userManagementView = new Administrator_UserManagement(_currentUser);

            userManagementView.BackToLibrariesRequested += ShowAdminLibraryManagement;

            MainContentArea.Content = userManagementView;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_currentUser == null)
            {
                return;
            }

            using (var db = new LibraryDbContext())
            {
                var user = db.Users.Find(_currentUser.Id);

                if (user != null)
                {
                    user.IsActive = false;
                    db.SaveChanges();
                }
            }
        }
    }
}