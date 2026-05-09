using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.UserControllers.Admin_UserControllers;
using LibraryManagement.UserControllers.Librarian_UserControllers;
using System.ComponentModel;
using System.Windows;

namespace LibraryManagement.Views
{
    /// <summary>
    /// Interaction logic for Application_MainWindow.xaml
    /// </summary>
    public partial class Application_MainWindow : Window
    {
        private Model_User _currentUser;
        public Application_MainWindow(Model_User user)
        {
            InitializeComponent();

            _currentUser = user;

            string role = (user.Role ?? string.Empty).Trim().ToLowerInvariant();

            if (role == "administrator" || role == "admin")
            {
                MainContentArea.Content = new Admin_UserController(user);
            }
            else if (role == "librarian")
            {
                MainContentArea.Content = new Librarian_UserController(user);
            }
            else
            {
                MessageBox.Show("Unknown user role: " + user.Role, "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

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
