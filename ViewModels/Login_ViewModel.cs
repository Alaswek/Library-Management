using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Views;
using LibraryManagement.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LibraryManagement.Models;
namespace LibraryManagement.ViewModels
{
    public class Login_ViewModel : ViewModelBase
    {
        private string username;
        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }

        private string show_ErrorMessage;
        public string Show_ErrorMessage
        {
            get => show_ErrorMessage;
            set { show_ErrorMessage = value; OnPropertyChanged(); }
        }

        private Model_User currentUser;
        public Model_User CurrentUser
        {
            get => currentUser;
            set { currentUser = value; OnPropertyChanged(); }
        }

        public ICommand Login_command { get; }

        public Login_ViewModel()
        {
            Login_command = new RelayCommand(param =>
            {
                var passwordBox = param as System.Windows.Controls.PasswordBox;
                this.Password = passwordBox?.Password;
                DoLogin();
            });
        }


        public void DoLogin()
        {
            using (var db = new LibraryDbContext())
            {
                try
                {
                    // Verificăm dacă există măcar un user în tabel
                    var totalUsers = db.Users.Count();
                    if (totalUsers == 0)
                    {
                        Show_ErrorMessage = "Baza de date este goala!";
                        return;
                    }

                    var user = db.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password);

                    if (user == null)
                    {
                        Show_ErrorMessage = "Wrong password or username!";
                        return;
                    }

                    // Succes
                    CurrentUser = user;
                    Application_MainWindow mainWin = new Application_MainWindow(CurrentUser);
                    mainWin.Show();

                    // Închide fereastra corectă (LoginApp_Window)
                    Application.Current.Windows.OfType<LoginApp_Window>().FirstOrDefault()?.Close();
                }
                catch (Exception ex)
                {
                    Show_ErrorMessage = "Eroare DB: " + ex.Message;
                }
            }
           
        }

    }
}
