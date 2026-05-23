using LibraryManagement.Models;
using LibraryManagement.ViewModels;
using System.Windows;
using System.Windows.Controls;
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


        private void RefreshDataFromViewModel()
        {
            var viewModel = DataContext as Librarian_LibraryManagement_ViewModel;

            if (viewModel == null)
            {
                return;
            }

            if (viewModel.RefreshDataCommand != null &&
                viewModel.RefreshDataCommand.CanExecute(null))
            {
                viewModel.RefreshDataCommand.Execute(null);
            }
        }

    }
}