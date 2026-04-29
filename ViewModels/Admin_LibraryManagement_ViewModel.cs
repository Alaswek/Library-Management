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
        private string _statusMessage;

        public AdminLibrariesViewModel(Model_User currentAdmin)
        {
            _currentAdmin = currentAdmin;
            _libraries = new ObservableCollection<Model_Library>();

            SaveLibraryCommand = new RelayCommand(_ => SaveLibrary());
            EditSelectedLibraryCommand = new RelayCommand(_ => StartEditingSelectedLibrary(), _ => SelectedLibrary != null);
            ClearFormCommand = new RelayCommand(_ => ResetForm());

            ResetForm();
            LoadLibraries();
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
            set { SetProperty(ref _selectedLibrary, value); }
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

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
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

        public ICommand SaveLibraryCommand { get; private set; }
        public ICommand EditSelectedLibraryCommand { get; private set; }
        public ICommand ClearFormCommand { get; private set; }

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

                StatusMessage = Libraries.Count == 0
                    ? "No libraries have been created yet."
                    : "Libraries loaded successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Libraries could not be loaded.";
                MessageBox.Show("Error loading libraries: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveLibrary()
        {
            if (string.IsNullOrWhiteSpace(LibraryName))
            {
                StatusMessage = "Library name is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(LibraryAddress))
            {
                StatusMessage = "Library address is required.";
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
                            StatusMessage = "The selected library no longer exists.";
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

                StatusMessage = _editingLibraryId > 0
                    ? "Library updated successfully."
                    : "Library created successfully.";

                LoadLibraries();
                ResetForm();
            }
            catch (Exception ex)
            {
                StatusMessage = "Library could not be saved.";
                MessageBox.Show("Error saving library: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartEditingSelectedLibrary()
        {
            if (SelectedLibrary == null)
            {
                StatusMessage = "Select a library to edit.";
                return;
            }

            _editingLibraryId = SelectedLibrary.Id;
            LibraryName = SelectedLibrary.Name;
            LibraryAddress = SelectedLibrary.Address;
            IsOpen = SelectedLibrary.IsOpen;
            StatusMessage = string.Format("Editing library: {0}", SelectedLibrary.Name);
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(FormTitle));
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
    }
}
