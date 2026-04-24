using LibraryManagement.Models;
using LibraryManagement.UserControllers.Admin_UserControllers;
using LibraryManagement.UserControllers.Librarian_UserControllers;
using System.Windows;

namespace LibraryManagement.Views
{
    /// <summary>
    /// Interaction logic for Application_MainWindow.xaml
    /// </summary>
    public partial class Application_MainWindow : Window
    {
        public Application_MainWindow(Model_User user)
        {
            InitializeComponent();

            string role = (user.Role ?? string.Empty).Trim().ToLowerInvariant();

            if (role == "administrator" || role == "admin")
            {
                MainContentArea.Content = new Admin_UserController(user);
            }
            else if (role == "librarian")
            {
                MainContentArea.Content = new Librarian_UserController();
            }
            else
            {
                MessageBox.Show("Unknown user role: " + user.Role, "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
            }
        }
    }
}
