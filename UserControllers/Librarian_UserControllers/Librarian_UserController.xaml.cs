using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace LibraryManagement.UserControllers.Librarian_UserControllers
{
    public partial class Librarian_UserController : UserControl
    {
        private Librarian_LibraryManagement_ViewModel _viewModel;

        public Librarian_UserController(Model_User currentLibrarian)
        {
            InitializeComponent();
            _viewModel = new Librarian_LibraryManagement_ViewModel(currentLibrarian);
            this.DataContext = _viewModel;
        }
    }
}