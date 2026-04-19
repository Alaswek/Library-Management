using LibraryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryManagement.UserControllers.Librarian_UserControllers
{
    /// <summary>
    /// Interaction logic for Librarian_UserController.xaml
    /// </summary>
    public partial class Librarian_UserController : UserControl
    {

        public Librarian_UserController()
        {
            InitializeComponent();
        }
        /*
        public Librarian_UserController(Model_Librarian librarian)
        {
            InitializeComponent();
            _currentLibrarian = librarian;
            LoadLibrarianData();
        } 

        private void LoadLibrarianData()
        {
            if (_currentLibrarian != null)
            {
                WelcomeTextBlock.Text = $"Welcome, {_currentLibrarian.Username}";
            }
        }
        */

        private void IssueBookButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Issue Book feature - Coming Soon!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ReturnBookButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Return Book feature - Coming Soon!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ManageInventoryButton_Click(Object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Inventory Management feature - Coming Soon!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ViewMembersButton_Click(Object sender, RoutedEventArgs e)
        {
            MessageBox.Show("View Members feature - Coming Soon!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void SearchBooksButton_Click(Object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Search Books feature - Coming Soon!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

// IMPORTANT:
// Pentru metode e nevoie sa punem in cod ce e o carte, ce e persoana care o imprumuta si ce inseamna imprumutul!!
// Ar trebui inca cateva modele pentru astea sa stiu ce logica sa pun in ele.
