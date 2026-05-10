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

        public AdminLibraryManagementViewModel()
            : this(null)
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

                return Libraries.Count(library => library.IsOpen);
            }
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
                MessageBox.Show("Error loading libraries: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Error saving library: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    var libraryId = libraryToDelete.Id;

                    var assignedLibrarians = db.Librarians.Any(librarian => librarian.Library_ID == libraryId);

                    if (assignedLibrarians)
                    {
                        LibraryStatusMessage = "This library cannot be deleted while librarians are assigned to it.";
                        return;
                    }

                    var hasBooks = db.Books.Any(book => book.LibraryId == libraryId);

                    if (hasBooks)
                    {
                        LibraryStatusMessage = "This library cannot be deleted while books are assigned to it.";
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

                LoadLibraries();
                ResetForm();
                LibraryStatusMessage = "Library deleted successfully.";
                RefreshCommandStates();
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
            SelectedLibrary = null;

            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(FormTitle));

            RefreshCommandStates();
        }
    }
}