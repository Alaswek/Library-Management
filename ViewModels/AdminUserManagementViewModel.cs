using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibraryManagement.ViewModels
{
    public class AdminUserManagementViewModel : ViewModelBase
    {
            public ObservableCollection<Model_User> Users { get; set; }

            private Model_User selectedUser;
            public Model_User SelectedUser
            {
                get => selectedUser;
                set
                {
                    selectedUser = value;
                    OnPropertyChanged();
                }
            }

            public ICommand AddUserCommand { get; }
            public ICommand UpdateUserCommand { get; }
            public ICommand DeleteUserCommand { get; }

            public AdminUserManagementViewModel()
            {
                LoadUsers();

                AddUserCommand = new RelayCommand(_ => AddUser());
                UpdateUserCommand = new RelayCommand(_ => UpdateUser());
                DeleteUserCommand = new RelayCommand(_ => DeleteUser());
            }

            private void LoadUsers()
            {
                using (var db = new LibraryDbContext())
                {
                    Users = new ObservableCollection<Model_User>(db.Users.ToList());
                }
            }

            private void AddUser()
            {
                using (var db = new LibraryDbContext())
                {
                    var user = new Model_User
                    {
                        Username = SelectedUser.Username,
                        Password = SelectedUser.Password,
                        Role = SelectedUser.Role,
                        IsActive = false,
                        Library_ID = SelectedUser.Library_ID
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    if (user.Role == "Administrator")
                    {
                        db.Administrators.Add(new Model_Administrator
                        {
                            UserId = user.Id
                        });
                    }
                    else if (user.Role == "Librarian")
                    {
                        db.Librarians.Add(new Model_Librarian
                        {
                            UserId = user.Id
                        });
                    }

                    db.SaveChanges();
                }

                LoadUsers();
            }

            private void UpdateUser()
            {
                using (var db = new LibraryDbContext())
                {
                    var user = db.Users.Find(SelectedUser.Id);

                    if (user != null)
                    {
                        user.Username = SelectedUser.Username;
                        user.Password = SelectedUser.Password;
                        user.Role = SelectedUser.Role;
                        user.Library_ID = SelectedUser.Library_ID;

                        db.SaveChanges();
                    }
                }

                LoadUsers();
            }

            private void DeleteUser()
            {
                using (var db = new LibraryDbContext())
                {
                    var user = db.Users.Find(SelectedUser.Id);

                    if (user != null)
                    {
                        var librarian = db.Librarians
                            .FirstOrDefault(x => x.UserId == user.Id);

                        if (librarian != null)
                            db.Librarians.Remove(librarian);

                        var admin = db.Administrators
                            .FirstOrDefault(x => x.UserId == user.Id);

                        if (admin != null)
                            db.Administrators.Remove(admin);

                        db.Users.Remove(user);

                        db.SaveChanges();
                    }
                }

                LoadUsers();
            }
    }
}

