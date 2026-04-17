using LibraryManagement.Models;
using Microsoft.VisualBasic.ApplicationServices;
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
using System.Windows.Shapes;

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
            if (user.Role.ToLower() == "administrator")
            {
                MainContentArea.Content = new Admin_UserController();
            }
            else if (user.Role.ToLower() == "librarian")
            {
                MainContentArea.Content = new Librarian_UserController();
            }
        }
    }
}
