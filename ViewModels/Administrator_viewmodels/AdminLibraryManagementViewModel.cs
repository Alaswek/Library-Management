using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LibraryManagement.ViewModels.Administrator_viewmodels
{
    public class AdminLibraryManagementViewModel : ViewModelBase
    {
        private readonly Model_User _currentAdmin;

        private ObservableCollection<Model_Library> _libraries;
        private Model_Library _selectedLibrary;
        private int _editingLibraryId;
        private string _libraryName;
        private string _libraryAddress;
        private bool _isOpen;
        private string _libraryStatusMessage;
        private string _openingHours;
        private bool _isInitialized;

        private const int LibraryNameMaxLength = 200;
        private const int LibraryAddressMaxLength = 300;
        private const int OpeningHoursMaxLength = 50;

        public AdminLibraryManagementViewModel() : this(null)
        {
        }

        public AdminLibraryManagementViewModel(Model_User currentAdmin)
        {
            _currentAdmin = currentAdmin;
            _libraries = new ObservableCollection<Model_Library>();

            SaveLibraryCommand = new RelayCommand(_ => SaveLibrary());

            EditSelectedLibraryCommand = new RelayCommand(
                parameter => StartEditingSelectedLibrary(parameter as Model_Library));

            DeleteSelectedLibraryCommand = new RelayCommand(
                parameter => DeleteSelectedLibrary(parameter as Model_Library));

            ClearFormCommand = new RelayCommand(_ => ResetForm());

            ResetForm();
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            LoadLibraries();
        }

        public string WelcomeMessage
        {
            get
            {
                if (_currentAdmin != null && !string.IsNullOrWhiteSpace(_currentAdmin.Username))
                {
                    return string.Format("Welcome, {0}", _currentAdmin.Username);
                }

                return "Welcome, Administrator";
            }
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
                    RefreshCommandStates();
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
            get
            {
                if (IsEditMode)
                {
                    return "Edit Library";
                }

                return "Create Library";
            }
        }

        public int TotalLibraries
        {
            get
            {
                if (Libraries == null)
                {
                    return 0;
                }

                return Libraries.Count;
            }
        }

        public int OpenLibraries
        {
            get
            {
                if (Libraries == null)
                {
                    return 0;
                }

                return Libraries.Count(library => library != null && library.IsOpen);
            }
        }

        public string OpeningHours
        {
            get { return _openingHours; }
            set { SetProperty(ref _openingHours, value); }
        }

        public ICommand SaveLibraryCommand { get; private set; }
        public ICommand EditSelectedLibraryCommand { get; private set; }
        public ICommand DeleteSelectedLibraryCommand { get; private set; }
        public ICommand ClearFormCommand { get; private set; }

        private void RefreshCommandStates()
        {
            CommandManager.InvalidateRequerySuggested();
        }

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

                SelectedLibrary = null;
                RefreshCommandStates();

                if (Libraries.Count == 0)
                {
                    LibraryStatusMessage = "No libraries have been created yet.";
                }
                else
                {
                    LibraryStatusMessage = "Libraries loaded successfully.";
                }
            }
            catch (Exception ex)
            {
                LibraryStatusMessage = "Libraries could not be loaded.";
                MessageBox.Show(
                    "Error loading libraries: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveLibrary()
        {
            string name = Normalize(LibraryName);
            string address = Normalize(LibraryAddress);
            string openingHours = Normalize(OpeningHours);

            if (string.IsNullOrWhiteSpace(name))
            {
                LibraryStatusMessage = "Library name is required.";
                return;
            }

            if (name.Length > LibraryNameMaxLength)
            {
                LibraryStatusMessage = string.Format(
                    "Library name cannot be longer than {0} characters.",
                    LibraryNameMaxLength);
                return;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                LibraryStatusMessage = "Library address is required.";
                return;
            }

            if (address.Length > LibraryAddressMaxLength)
            {
                LibraryStatusMessage = string.Format(
                    "Library address cannot be longer than {0} characters.",
                    LibraryAddressMaxLength);
                return;
            }

            if (string.IsNullOrWhiteSpace(openingHours))
            {
                LibraryStatusMessage = "Opening hours are required.";
                return;
            }

            if (openingHours.Length > OpeningHoursMaxLength)
            {
                LibraryStatusMessage = string.Format(
                    "Opening hours cannot be longer than {0} characters.",
                    OpeningHoursMaxLength);
                return;
            }

            try
            {
                bool wasEditMode = _editingLibraryId > 0;

                using (var db = new LibraryDbContext())
                {
                    Model_Library library;

                    if (_editingLibraryId > 0)
                    {
                        library = db.Libraries.FirstOrDefault(item => item.Id == _editingLibraryId);

                        if (library == null)
                        {
                            LibraryStatusMessage = "The selected library no longer exists.";
                            ResetForm();
                            LoadLibraries();
                            return;
                        }
                    }
                    else
                    {
                        library = new Model_Library();
                        db.Libraries.Add(library);
                    }

                    string normalizedName = name.ToLowerInvariant();

                    bool duplicateNameExists = db.Libraries.Any(item =>
                        item.Id != _editingLibraryId &&
                        item.Name != null &&
                        item.Name.ToLower() == normalizedName);

                    if (duplicateNameExists)
                    {
                        LibraryStatusMessage = "A library with this name already exists.";
                        return;
                    }

                    library.Name = name;
                    library.Address = address;
                    library.OpeningHours = openingHours;
                    library.IsOpen = IsOpen;

                    db.SaveChanges();
                }

                string successMessage;

                if (wasEditMode)
                {
                    successMessage = "Library updated successfully.";
                }
                else
                {
                    successMessage = "Library created successfully.";
                }

                LoadLibraries();
                ResetForm();
                LibraryStatusMessage = successMessage;
                RefreshCommandStates();
            }
            catch (Exception ex)
            {
                LibraryStatusMessage = "Library could not be saved.";
                MessageBox.Show(
                    "Error saving library: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void StartEditingSelectedLibrary(Model_Library libraryFromCommand)
        {
            Model_Library libraryToEdit;

            if (libraryFromCommand != null)
            {
                libraryToEdit = libraryFromCommand;
            }
            else
            {
                libraryToEdit = SelectedLibrary;
            }

            if (libraryToEdit == null)
            {
                LibraryStatusMessage = "Select a library to edit.";
                return;
            }

            SelectedLibrary = libraryToEdit;

            _editingLibraryId = libraryToEdit.Id;
            LibraryName = libraryToEdit.Name;
            LibraryAddress = libraryToEdit.Address;
            OpeningHours = libraryToEdit.OpeningHours;
            IsOpen = libraryToEdit.IsOpen;

            LibraryStatusMessage = string.Format("Editing library: {0}", libraryToEdit.Name);

            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(FormTitle));

            RefreshCommandStates();
        }

        private void DeleteSelectedLibrary(Model_Library libraryFromCommand)
        {
            Model_Library libraryToDelete;

            if (libraryFromCommand != null)
            {
                libraryToDelete = libraryFromCommand;
            }
            else
            {
                libraryToDelete = SelectedLibrary;
            }

            if (libraryToDelete == null)
            {
                LibraryStatusMessage = "Select a library to delete.";
                return;
            }

            SelectedLibrary = libraryToDelete;

            var confirmation = MessageBox.Show(
                string.Format("Are you sure you want to delete the library \"{0}\"?", libraryToDelete.Name),
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                LibraryStatusMessage = "Delete operation was cancelled.";
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    int libraryId = libraryToDelete.Id;

                    bool assignedLibrarians = db.Librarians.Any(librarian => librarian.Library_ID == libraryId);

                    if (assignedLibrarians)
                    {
                        LibraryStatusMessage = "This library cannot be deleted while librarians are assigned to it.";
                        return;
                    }

                    bool hasBooks = db.Books.Any(book => book.LibraryId == libraryId);

                    if (hasBooks)
                    {
                        LibraryStatusMessage = "This library cannot be deleted while books are assigned to it.";
                        return;
                    }

                    bool hasRentals = db.Rentals.Any(rental =>
                        db.Books.Any(book => book.Id == rental.BookId && book.LibraryId == libraryId));

                    if (hasRentals)
                    {
                        LibraryStatusMessage = "This library cannot be deleted while rentals are assigned to it.";
                        return;
                    }

                    var library = db.Libraries.FirstOrDefault(item => item.Id == libraryId);

                    if (library == null)
                    {
                        LibraryStatusMessage = "The selected library no longer exists.";
                        ResetForm();
                        LoadLibraries();
                        return;
                    }

                    db.Libraries.Remove(library);
                    db.SaveChanges();
                }

                LoadLibraries();
                ResetForm();
                LibraryStatusMessage = "Library deleted successfully.";
                RefreshCommandStates();
            }
            catch (Exception ex)
            {
                LibraryStatusMessage = "Library could not be deleted.";
                MessageBox.Show(
                    "Error deleting library: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            _editingLibraryId = 0;
            LibraryName = string.Empty;
            LibraryAddress = string.Empty;
            OpeningHours = string.Empty;

            IsOpen = true;

            SelectedLibrary = null;

            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(FormTitle));

            RefreshCommandStates();
        }

        private static string Normalize(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Trim();
        }
    }
}
