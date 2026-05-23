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
        private bool _isReturningToLogin = false;

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
            libraryView.OpenLibraryBooksRequested += ShowAdminLibraryBooksManagement;

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
            SetCurrentUserInactive();

            if (!_isReturningToLogin)
            {
                _isReturningToLogin = true;

                var loginWindow = new LoginAppl_Window();

                Application.Current.MainWindow = loginWindow;

                loginWindow.Show();
            }

            base.OnClosing(e);
        }

        private void SetCurrentUserInactive()
        {
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

        private void ShowAdminLibraryBooksManagement(Model_Library selectedLibrary)
        {
            if (selectedLibrary == null)
            {
                MessageBox.Show(
                    "Please select a library first.",
                    "No Library Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var booksView = new AdminLibraryBooks_UserController(_currentUser, selectedLibrary);

            booksView.BackToLibrariesRequested += ShowAdminLibraryManagement;

            MainContentArea.Content = booksView;
        }
    }
}