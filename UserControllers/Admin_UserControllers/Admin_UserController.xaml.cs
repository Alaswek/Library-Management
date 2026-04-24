using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using System.Windows.Controls;

namespace LibraryManagement.UserControllers.Admin_UserControllers
{
    public partial class Admin_UserController : UserControl
    {
        public Admin_UserController(Model_User currentUser)
        {
            InitializeComponent();
            DataContext = new Admin_LibraryManagement_ViewModel(currentUser);
        }
    }
}
