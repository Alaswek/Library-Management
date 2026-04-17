using LibraryManagement.ViewModels;
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
    /// Interaction logic for LoginApp_Window.xaml
    /// </summary>
    public partial class LoginApp_Window : Window
    {
        public LoginApp_Window()
        {
            InitializeComponent();
        }

        // Permite mutarea ferestrei cu mouse-ul (tragere de bara de sus)
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Butonul Close (X)
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Butonul Maximize
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        // Butonul Minimize
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
<<<<<<< Updated upstream
=======
        
        // Gestionare Focus Username
        private void Username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (usernameBox.Text == "Username")
                usernameBox.Text = "";
        }

        private void Username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameBox.Text))
                usernameBox.Text = "Username";
        }
>>>>>>> Stashed changes
    }
}