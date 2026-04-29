using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LibraryManagement.ViewModels
{
    public class AdminLibrariesViewModel : ViewModelBase
    {
        private readonly Model_User _currentAdmin;
        private ObservableCollection<Model_Library> _libraries;
        private Model_Library _selectedLibrary;
        private int _editingLibraryId;
        private string _libraryName;
        private string _libraryAddress;
        private bool _isOpen;
        private string _libraryStatusMessage;
        private ObservableCollection<Model_Librarian> _librarians;
        private Model_Librarian _selectedLibrarian;
        private int _editingLibrarianId;
        private string _librarianUsername;
        private string _librarianPassword;
        private int _selectedLibrarianLibraryId;
        private bool _isLibrarianActive;
        private string _librarianStatusMessage;

        public AdminLibrariesViewModel(Model_User currentAdmin)
        {
            _currentAdmin = currentAdmin;
            _libraries = new ObservableCollection<Model_Library>();
            _librarians = new ObservableCollection<Model_Librarian>();

            SaveLibraryCommand = new RelayCommand(_ => SaveLibrary());
            EditSelectedLibraryCommand = new RelayCommand(_ => StartEditingSelectedLibrary(), _ => SelectedLibrary != null);
            DeleteSelectedLibraryCommand = new RelayCommand(_ => DeleteSelectedLibrary(), _ => SelectedLibrary != null);
            ClearFormCommand = new RelayCommand(_ => ResetForm());
            SaveLibrarianCommand = new RelayCommand(_ => SaveLibrarian());
            EditSelectedLibrarianCommand = new RelayCommand(_ => StartEditingSelectedLibrarian(), _ => SelectedLibrarian != null);
            DeleteSelectedLibrarianCommand = new RelayCommand(_ => DeleteSelectedLibrarian(), _ => SelectedLibrarian != null);
            ClearLibrarianFormCommand = new RelayCommand(_ => ResetLibrarianForm());

            ResetForm();
            ResetLibrarianForm();
            LoadLibraries();
            LoadLibrarians();
        }

        public string WelcomeMessage
        {
            get { return string.Format("Welcome, {0}", _currentAdmin != null ? _currentAdmin.Username : "Administrator"); }
        }

        public ObservableCollection<Model_Library> Libraries
        {
            get { return _libraries; }
            set
            {
                if (SetProperty(ref _libraries, value))
                {
                    OnPropertyChanged(nameof(TotalLibraries));
                    OnPropertyChanged(nameof(OpenLibraries));
                }
            }
        }

        public Model_Library SelectedLibrary
        {
            get { return _selectedLibrary; }
            set
            {
                if (SetProperty(ref _selectedLibrary, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string LibraryName
        {
            get { return _libraryName; }
            set { SetProperty(ref _libraryName, value); }
        }

        public string LibraryAddress
        {
            get { return _libraryAddress; }
            set { SetProperty(ref _libraryAddress, value); }
        }

        public bool IsOpen
        {
            get { return _isOpen; }
            set { SetProperty(ref _isOpen, value); }
        }

        public string LibraryStatusMessage
        {
            get { return _libraryStatusMessage; }
            set { SetProperty(ref _libraryStatusMessage, value); }
        }

        public bool IsEditMode
        {
            get { return _editingLibraryId > 0; }
        }

        public string FormTitle
        {
            get { return IsEditMode ? "Edit Library" : "Create Library"; }
        }

        public int TotalLibraries
        {
            get { return Libraries.Count; }
        }

        public int OpenLibraries
        {
            get { return Libraries.Count(library => library.IsOpen); }
        }

        public ObservableCollection<Model_Librarian> Librarians
        {
            get { return _librarians; }
            set
            {
                if (SetProperty(ref _librarians, value))
                {
                    OnPropertyChanged(nameof(TotalLibrarians));
                    OnPropertyChanged(nameof(ActiveLibrarians));
                }
            }
        }

        public Model_Librarian SelectedLibrarian
        {
            get { return _selectedLibrarian; }
            set
            {
                if (SetProperty(ref _selectedLibrarian, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string LibrarianUsername
        {
            get { return _librarianUsername; }
            set { SetProperty(ref _librarianUsername, value); }
        }

        public string LibrarianPassword
        {
            get { return _librarianPassword; }
            set { SetProperty(ref _librarianPassword, value); }
        }

        public int SelectedLibrarianLibraryId
        {
            get { return _selectedLibrarianLibraryId; }
            set { SetProperty(ref _selectedLibrarianLibraryId, value); }
        }

        public bool IsLibrarianActive
        {
            get { return _isLibrarianActive; }
            set { SetProperty(ref _isLibrarianActive, value); }
        }

        public string LibrarianStatusMessage
        {
            get { return _librarianStatusMessage; }
            set { SetProperty(ref _librarianStatusMessage, value); }
        }

        public bool IsLibrarianEditMode
        {
            get { return _editingLibrarianId > 0; }
        }

        public string LibrarianFormTitle
        {
            get { return IsLibrarianEditMode ? "Edit Librarian Account" : "Create Librarian Account"; }
        }

        public int TotalLibrarians
        {
            get { return Librarians.Count; }
        }

        public int ActiveLibrarians
        {
            get { return Librarians.Count(librarian => librarian.IsActive); }
        }

        public ICommand SaveLibraryCommand { get; private set; }
        public ICommand EditSelectedLibraryCommand { get; private set; }
        public ICommand DeleteSelectedLibraryCommand { get; private set; }
        public ICommand ClearFormCommand { get; private set; }
        public ICommand SaveLibrarianCommand { get; private set; }
        public ICommand EditSelectedLibrarianCommand { get; private set; }
        public ICommand DeleteSelectedLibrarianCommand { get; private set; }
        public ICommand ClearLibrarianFormCommand { get; private set; }

        private void LoadLibraries()
        {
            try
            {
                using (var db = new LibraryDbContext())
                {
                    Libraries = new ObservableCollection<Model_Library>(
                        db.Libraries
                          .OrderBy(library => library.Name)
                          .ToList());
                }

                LibraryStatusMessage = Libraries.Count == 0
                    ? "No libraries have been created yet."
                    : "Libraries loaded successfully.";
            }
            catch (Exception ex)
            {
                LibraryStatusMessage = "Libraries could not be loaded.";
                MessageBox.Show("Error loading libraries: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLibrarians()
        {
            try
            {
                using (var db = new LibraryDbContext())
                {
                    var librarians = db.Database.SqlQuery<Model_Librarian>(
                        @"SELECT u.[Id],
                                 u.[Username],
                                 u.[Password],
                                 u.[Role],
                                 u.[IsActive],
                                 ISNULL(u.[Library_ID], 0) AS [Library_ID],
                                 ISNULL(l.[Name], N'Unassigned') AS [LibraryName]
                          FROM [dbo].[Users] u
                          LEFT JOIN [dbo].[Libraries] l ON l.[Id] = u.[Library_ID]
                          WHERE LOWER(LTRIM(RTRIM(u.[Role]))) = @p0
                          ORDER BY u.[Username]",
                        "librarian")
                        .ToList();

                    Librarians = new ObservableCollection<Model_Librarian>(librarians);
                }

                LibrarianStatusMessage = Librarians.Count == 0
                    ? "No librarian accounts have been created yet."
                    : "Librarian accounts loaded successfully.";
            }
            catch (Exception ex)
            {
                LibrarianStatusMessage = "Librarian accounts could not be loaded.";
                MessageBox.Show("Error loading librarian accounts: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveLibrary()
        {
            if (string.IsNullOrWhiteSpace(LibraryName))
            {
                LibraryStatusMessage = "Library name is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(LibraryAddress))
            {
                LibraryStatusMessage = "Library address is required.";
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    Model_Library library;

                    if (_editingLibraryId > 0)
                    {
                        library = db.Libraries.FirstOrDefault(item => item.Id == _editingLibraryId);
                        if (library == null)
                        {
                            LibraryStatusMessage = "The selected library no longer exists.";
                            return;
                        }
                    }
                    else
                    {
                        library = new Model_Library();
                        db.Libraries.Add(library);
                    }

                    library.Name = LibraryName.Trim();
                    library.Address = LibraryAddress.Trim();
                    library.IsOpen = IsOpen;

                    db.SaveChanges();
                }

                var successMessage = _editingLibraryId > 0
                    ? "Library updated successfully."
                    : "Library created successfully.";

                LoadLibraries();
                LoadLibrarians();
                ResetForm();
                LibraryStatusMessage = successMessage;
            }
            catch (Exception ex)
            {
                LibraryStatusMessage = "Library could not be saved.";
                MessageBox.Show("Error saving library: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartEditingSelectedLibrary()
        {
            if (SelectedLibrary == null)
            {
                LibraryStatusMessage = "Select a library to edit.";
                return;
            }

            _editingLibraryId = SelectedLibrary.Id;
            LibraryName = SelectedLibrary.Name;
            LibraryAddress = SelectedLibrary.Address;
            IsOpen = SelectedLibrary.IsOpen;
            LibraryStatusMessage = string.Format("Editing library: {0}", SelectedLibrary.Name);
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(FormTitle));
        }

        private void DeleteSelectedLibrary()
        {
            if (SelectedLibrary == null)
            {
                LibraryStatusMessage = "Select a library to delete.";
                return;
            }

            var confirmation = MessageBox.Show(
                string.Format("Are you sure you want to delete the library \"{0}\"?", SelectedLibrary.Name),
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    var libraryId = SelectedLibrary.Id;
                    var assignedLibrarians = db.Database.SqlQuery<int>(
                        @"SELECT COUNT(*)
                          FROM [dbo].[Users]
                          WHERE LOWER(LTRIM(RTRIM([Role]))) = @p0
                            AND [Library_ID] = @p1",
                        "librarian",
                        libraryId)
                        .Single();

                    if (assignedLibrarians > 0)
                    {
                        LibraryStatusMessage = "This library cannot be deleted while librarians are assigned to it.";
                        return;
                    }

                    var library = db.Libraries.FirstOrDefault(item => item.Id == libraryId);
                    if (library == null)
                    {
                        LibraryStatusMessage = "The selected library no longer exists.";
                        return;
                    }

                    db.Libraries.Remove(library);
                    db.SaveChanges();
                }

                var successMessage = "Library deleted successfully.";
                LoadLibraries();
                LoadLibrarians();
                ResetForm();
                SelectedLibrary = null;
                LibraryStatusMessage = successMessage;
            }
            catch (Exception ex)
            {
                LibraryStatusMessage = "Library could not be deleted.";
                MessageBox.Show("Error deleting library: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            _editingLibraryId = 0;
            LibraryName = string.Empty;
            LibraryAddress = string.Empty;
            IsOpen = true;
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(FormTitle));
        }

        private void SaveLibrarian()
        {
            if (string.IsNullOrWhiteSpace(LibrarianUsername))
            {
                LibrarianStatusMessage = "Librarian username is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(LibrarianPassword))
            {
                LibrarianStatusMessage = "Librarian password is required.";
                return;
            }

            if (SelectedLibrarianLibraryId <= 0)
            {
                LibrarianStatusMessage = "Assign the librarian to a library.";
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    var trimmedUsername = LibrarianUsername.Trim();
                    var usernameExists = db.Users
                        .ToList()
                        .Any(user => user.Id != _editingLibrarianId &&
                                     string.Equals((user.Username ?? string.Empty).Trim(), trimmedUsername, StringComparison.OrdinalIgnoreCase));

                    if (usernameExists)
                    {
                        LibrarianStatusMessage = "This username is already used by another account.";
                        return;
                    }

                    if (!db.Libraries.Any(library => library.Id == SelectedLibrarianLibraryId))
                    {
                        LibrarianStatusMessage = "The selected library no longer exists.";
                        return;
                    }

                    if (_editingLibrarianId > 0)
                    {
                        var affectedRows = db.Database.ExecuteSqlCommand(
                            @"UPDATE [dbo].[Users]
                              SET [Username] = @p0,
                                  [Password] = @p1,
                                  [Role] = @p2,
                                  [IsActive] = @p3,
                                  [Library_ID] = @p4
                              WHERE [Id] = @p5
                                AND LOWER(LTRIM(RTRIM([Role]))) = @p6",
                            trimmedUsername,
                            LibrarianPassword,
                            "librarian",
                            IsLibrarianActive,
                            SelectedLibrarianLibraryId,
                            _editingLibrarianId,
                            "librarian");

                        if (affectedRows == 0)
                        {
                            LibrarianStatusMessage = "The selected librarian account no longer exists.";
                            return;
                        }
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand(
                            @"INSERT INTO [dbo].[Users] ([Username], [Password], [Role], [IsActive], [Library_ID])
                              VALUES (@p0, @p1, @p2, @p3, @p4)",
                            trimmedUsername,
                            LibrarianPassword,
                            "librarian",
                            IsLibrarianActive,
                            SelectedLibrarianLibraryId);
                    }
                }

                var successMessage = _editingLibrarianId > 0
                    ? "Librarian account updated successfully."
                    : "Librarian account created successfully.";

                LoadLibrarians();
                ResetLibrarianForm();
                LibrarianStatusMessage = successMessage;
            }
            catch (Exception ex)
            {
                LibrarianStatusMessage = "Librarian account could not be saved.";
                MessageBox.Show("Error saving librarian account: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartEditingSelectedLibrarian()
        {
            if (SelectedLibrarian == null)
            {
                LibrarianStatusMessage = "Select a librarian account to edit.";
                return;
            }

            _editingLibrarianId = SelectedLibrarian.Id;
            LibrarianUsername = SelectedLibrarian.Username;
            LibrarianPassword = SelectedLibrarian.Password;
            SelectedLibrarianLibraryId = SelectedLibrarian.Library_ID;
            IsLibrarianActive = SelectedLibrarian.IsActive;
            LibrarianStatusMessage = string.Format("Editing librarian account: {0}", SelectedLibrarian.Username);
            OnPropertyChanged(nameof(IsLibrarianEditMode));
            OnPropertyChanged(nameof(LibrarianFormTitle));
        }

        private void DeleteSelectedLibrarian()
        {
            if (SelectedLibrarian == null)
            {
                LibrarianStatusMessage = "Select a librarian account to delete.";
                return;
            }

            var confirmation = MessageBox.Show(
                string.Format("Are you sure you want to delete the librarian account \"{0}\"?", SelectedLibrarian.Username),
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    var affectedRows = db.Database.ExecuteSqlCommand(
                        @"DELETE FROM [dbo].[Users]
                          WHERE [Id] = @p0
                            AND LOWER(LTRIM(RTRIM([Role]))) = @p1",
                        SelectedLibrarian.Id,
                        "librarian");

                    if (affectedRows == 0)
                    {
                        LibrarianStatusMessage = "The selected librarian account no longer exists.";
                        return;
                    }
                }

                var successMessage = "Librarian account deleted successfully.";
                LoadLibrarians();
                ResetLibrarianForm();
                SelectedLibrarian = null;
                LibrarianStatusMessage = successMessage;
            }
            catch (Exception ex)
            {
                LibrarianStatusMessage = "Librarian account could not be deleted.";
                MessageBox.Show("Error deleting librarian account: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetLibrarianForm()
        {
            _editingLibrarianId = 0;
            LibrarianUsername = string.Empty;
            LibrarianPassword = string.Empty;
            SelectedLibrarianLibraryId = 0;
            IsLibrarianActive = true;
            OnPropertyChanged(nameof(IsLibrarianEditMode));
            OnPropertyChanged(nameof(LibrarianFormTitle));
        }

        private bool IsLibrarianRole(string role)
        {
            return string.Equals((role ?? string.Empty).Trim(), "librarian", StringComparison.OrdinalIgnoreCase);
        }
    }
}
