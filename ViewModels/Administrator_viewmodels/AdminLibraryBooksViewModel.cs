using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace LibraryManagement.ViewModels.Administrator_viewmodels
{
    public class AdminLibraryBooksViewModel : ViewModelBase
    {
        #region Fields

        private readonly Model_User _currentAdmin;
        private readonly Model_Library _selectedLibrary;

        private ObservableCollection<Model_Book> _books;
        private ObservableCollection<Model_Book> _filteredBooks;
        private ObservableCollection<Model_Rental> _activeRentals;
        private ObservableCollection<Model_Rental> _rentalHistory;
        private ObservableCollection<Model_Member> _members;
        private ObservableCollection<string> _categories;

        private ObservableCollection<Model_Member> _memberOverviewResults;
        private ObservableCollection<Model_Rental> _selectedMemberActiveRentals;
        private ObservableCollection<Model_Rental> _selectedMemberRentalHistory;

        private string _searchText;
        private string _memberSearchText;
        private string _rentMemberStudentId;
        private string _rentMemberFullName;
        private string _rentMemberEmail;
        private string _rentMemberPhone;
        private string _selectedCategory;

        private Model_Book _selectedBook;
        private Model_Member _selectedMember;
        private Model_Rental _selectedRental;

        private DateTime? _rentalDueDate;
        private string _bookStatusMessage;
        private string _rentalStatusMessage;
        private bool _showRentalHistory;

        private Model_Book _newBook;
        private Model_Book _editingBook;
        private bool _isAddingBook;
        private bool _isEditingBook;
        private bool _isBookFormReadOnly;

        private string _memberOverviewSearchText;
        private Model_Member _selectedOverviewMember;

        #endregion

        #region Constructors

        public AdminLibraryBooksViewModel()
            : this(null, null)
        {
        }

        public AdminLibraryBooksViewModel(Model_User currentAdmin, Model_Library selectedLibrary)
        {
            _currentAdmin = currentAdmin;
            _selectedLibrary = selectedLibrary;

            _rentalDueDate = DateTime.Now.AddDays(14);
            _showRentalHistory = false;

            _books = new ObservableCollection<Model_Book>();
            _filteredBooks = new ObservableCollection<Model_Book>();
            _activeRentals = new ObservableCollection<Model_Rental>();
            _rentalHistory = new ObservableCollection<Model_Rental>();
            _members = new ObservableCollection<Model_Member>();
            _categories = new ObservableCollection<string>();

            _memberOverviewResults = new ObservableCollection<Model_Member>();
            _selectedMemberActiveRentals = new ObservableCollection<Model_Rental>();
            _selectedMemberRentalHistory = new ObservableCollection<Model_Rental>();

            _newBook = CreateEmptyBookForm();
            _editingBook = CreateEmptyBookForm();

            DeleteBookCommand = new RelayCommand(_ => DeleteBook());
            RentBookCommand = new RelayCommand(_ => RentBook());
            ReturnBookCommand = new RelayCommand(parameter => ReturnBook(parameter as Model_Rental));
            SearchMembersCommand = new RelayCommand(_ => SearchMembers());
            RefreshDataCommand = new RelayCommand(_ => LoadData());

            AddBookCommand = new RelayCommand(_ => ClearBookForm());
            SelectBookCommand = new RelayCommand(parameter => SelectBook(parameter as Model_Book));
            SaveBookCommand = new RelayCommand(_ => SaveBookFromForm());
            EditBookCommand = new RelayCommand(parameter => LoadBookIntoForm(parameter as Model_Book));
            CancelBookCommand = new RelayCommand(_ => ClearBookForm());
            ViewRentalHistoryCommand = new RelayCommand(_ => ToggleRentalHistory());

            SearchMemberOverviewCommand = new RelayCommand(_ => SearchMemberOverview());
            ViewMemberOverviewCommand = new RelayCommand(parameter => ViewMemberOverview(parameter as Model_Member));

            LoadData();
        }

        #endregion

        #region General properties

        public string WelcomeMessage
        {
            get
            {
                string adminName = "Administrator";

                if (_currentAdmin != null && !string.IsNullOrWhiteSpace(_currentAdmin.Username))
                {
                    adminName = _currentAdmin.Username;
                }

                if (_selectedLibrary != null && !string.IsNullOrWhiteSpace(_selectedLibrary.Name))
                {
                    return string.Format(
                        "Welcome, {0}!  ---    Managing books for library: {1}",
                        adminName,
                        _selectedLibrary.Name);
                }

                return string.Format(
                    "Welcome, {0}!  ---    No library selected.",
                    adminName);
            }
        }

        public ObservableCollection<Model_Book> Books
        {
            get { return _books; }
            set
            {
                if (SetProperty(ref _books, value))
                {
                    OnPropertyChanged(nameof(TotalBooks));
                    OnPropertyChanged(nameof(AvailableBooksCount));
                    FilterBooks();
                }
            }
        }

        public ObservableCollection<Model_Book> FilteredBooks
        {
            get { return _filteredBooks; }
            set { SetProperty(ref _filteredBooks, value); }
        }

        public ObservableCollection<Model_Rental> ActiveRentals
        {
            get { return _activeRentals; }
            set
            {
                if (SetProperty(ref _activeRentals, value))
                {
                    OnPropertyChanged(nameof(ActiveRentalsCount));
                }
            }
        }

        public ObservableCollection<Model_Rental> RentalHistory
        {
            get { return _rentalHistory; }
            set { SetProperty(ref _rentalHistory, value); }
        }

        public ObservableCollection<Model_Member> Members
        {
            get { return _members; }
            set { SetProperty(ref _members, value); }
        }

        public ObservableCollection<string> Categories
        {
            get { return _categories; }
            set { SetProperty(ref _categories, value); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterBooks();
                }
            }
        }

        public string MemberSearchText
        {
            get { return _memberSearchText; }
            set { SetProperty(ref _memberSearchText, value); }
        }

        public string RentMemberStudentId
        {
            get { return _rentMemberStudentId; }
            set { SetProperty(ref _rentMemberStudentId, value); }
        }

        public string RentMemberFullName
        {
            get { return _rentMemberFullName; }
            set { SetProperty(ref _rentMemberFullName, value); }
        }

        public string RentMemberEmail
        {
            get { return _rentMemberEmail; }
            set { SetProperty(ref _rentMemberEmail, value); }
        }

        public string RentMemberPhone
        {
            get { return _rentMemberPhone; }
            set { SetProperty(ref _rentMemberPhone, value); }
        }

        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterBooks();
                }
            }
        }

        public Model_Book SelectedBook
        {
            get { return _selectedBook; }
            set
            {
                if (SetProperty(ref _selectedBook, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Model_Member SelectedMember
        {
            get { return _selectedMember; }
            set
            {
                if (SetProperty(ref _selectedMember, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Model_Rental SelectedRental
        {
            get { return _selectedRental; }
            set
            {
                if (SetProperty(ref _selectedRental, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public DateTime? RentalDueDate
        {
            get { return _rentalDueDate; }
            set
            {
                if (SetProperty(ref _rentalDueDate, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string BookStatusMessage
        {
            get { return _bookStatusMessage; }
            set { SetProperty(ref _bookStatusMessage, value); }
        }

        public string RentalStatusMessage
        {
            get { return _rentalStatusMessage; }
            set { SetProperty(ref _rentalStatusMessage, value); }
        }

        public bool ShowRentalHistory
        {
            get { return _showRentalHistory; }
            set
            {
                if (SetProperty(ref _showRentalHistory, value))
                {
                    if (value)
                    {
                        LoadRentalHistory();
                    }

                    OnPropertyChanged(nameof(RentalHistoryButtonText));
                }
            }
        }

        public string RentalHistoryButtonText
        {
            get { return _showRentalHistory ? "Show Active Rentals" : "Show Rental History"; }
        }

        #endregion

        #region Book form properties

        public Model_Book NewBook
        {
            get { return _newBook; }
            set { SetProperty(ref _newBook, value); }
        }

        public Model_Book EditingBook
        {
            get { return _editingBook; }
            set
            {
                if (SetProperty(ref _editingBook, value))
                {
                    OnPropertyChanged(nameof(BookSaveButtonText));
                }
            }
        }

        public string BookSaveButtonText
        {
            get
            {
                if (EditingBook != null && EditingBook.Id > 0)
                {
                    return "SAVE CHANGES";
                }

                return "ADD BOOK";
            }
        }

        public bool IsAddingBook
        {
            get { return _isAddingBook; }
            set { SetProperty(ref _isAddingBook, value); }
        }

        public bool IsEditingBook
        {
            get { return _isEditingBook; }
            set { SetProperty(ref _isEditingBook, value); }
        }

        public bool IsBookFormReadOnly
        {
            get { return _isBookFormReadOnly; }
            set
            {
                if (SetProperty(ref _isBookFormReadOnly, value))
                {
                    OnPropertyChanged(nameof(IsBookFormEditable));
                }
            }
        }

        public bool IsBookFormEditable
        {
            get { return !IsBookFormReadOnly; }
        }

        #endregion

        #region Statistics properties

        public int TotalBooks
        {
            get { return Books?.Count ?? 0; }
        }

        public int AvailableBooksCount
        {
            get { return Books?.Count(book => book.AvailableQuantity > 0) ?? 0; }
        }

        public int ActiveRentalsCount
        {
            get { return ActiveRentals?.Count ?? 0; }
        }

        #endregion

        #region Member overview properties

        public string MemberOverviewSearchText
        {
            get { return _memberOverviewSearchText; }
            set { SetProperty(ref _memberOverviewSearchText, value); }
        }

        public Model_Member SelectedOverviewMember
        {
            get { return _selectedOverviewMember; }
            set
            {
                if (SetProperty(ref _selectedOverviewMember, value))
                {
                    OnPropertyChanged(nameof(SelectedOverviewMemberName));
                    OnPropertyChanged(nameof(SelectedOverviewMemberEmail));
                    OnPropertyChanged(nameof(SelectedOverviewMemberPhone));
                    OnPropertyChanged(nameof(SelectedOverviewMemberCode));
                    OnPropertyChanged(nameof(SelectedMemberActiveRentalsCount));
                    OnPropertyChanged(nameof(SelectedMemberOverdueRentalsCount));
                    OnPropertyChanged(nameof(SelectedMemberTotalRentalsCount));
                }
            }
        }

        public ObservableCollection<Model_Member> MemberOverviewResults
        {
            get { return _memberOverviewResults; }
            set { SetProperty(ref _memberOverviewResults, value); }
        }

        public ObservableCollection<Model_Rental> SelectedMemberActiveRentals
        {
            get { return _selectedMemberActiveRentals; }
            set
            {
                if (SetProperty(ref _selectedMemberActiveRentals, value))
                {
                    OnPropertyChanged(nameof(SelectedMemberActiveRentalsCount));
                    OnPropertyChanged(nameof(SelectedMemberOverdueRentalsCount));
                }
            }
        }

        public ObservableCollection<Model_Rental> SelectedMemberRentalHistory
        {
            get { return _selectedMemberRentalHistory; }
            set
            {
                if (SetProperty(ref _selectedMemberRentalHistory, value))
                {
                    OnPropertyChanged(nameof(SelectedMemberTotalRentalsCount));
                }
            }
        }

        public string SelectedOverviewMemberName
        {
            get
            {
                if (SelectedOverviewMember == null)
                {
                    return "No member selected";
                }

                return SelectedOverviewMember.FullName;
            }
        }

        public string SelectedOverviewMemberEmail
        {
            get
            {
                if (SelectedOverviewMember == null)
                {
                    return "-";
                }

                if (string.IsNullOrWhiteSpace(SelectedOverviewMember.Email))
                {
                    return "-";
                }

                return SelectedOverviewMember.Email;
            }
        }

        public string SelectedOverviewMemberPhone
        {
            get
            {
                if (SelectedOverviewMember == null)
                {
                    return "-";
                }

                if (string.IsNullOrWhiteSpace(SelectedOverviewMember.Phone))
                {
                    return "-";
                }

                return SelectedOverviewMember.Phone;
            }
        }

        public string SelectedOverviewMemberCode
        {
            get
            {
                if (SelectedOverviewMember == null)
                {
                    return "-";
                }

                if (string.IsNullOrWhiteSpace(SelectedOverviewMember.StudentId))
                {
                    return "-";
                }

                return SelectedOverviewMember.StudentId;
            }
        }

        public int SelectedMemberActiveRentalsCount
        {
            get
            {
                if (SelectedMemberActiveRentals == null)
                {
                    return 0;
                }

                return SelectedMemberActiveRentals.Count;
            }
        }

        public int SelectedMemberOverdueRentalsCount
        {
            get
            {
                if (SelectedMemberActiveRentals == null)
                {
                    return 0;
                }

                return SelectedMemberActiveRentals.Count(rental => rental.DueDate < DateTime.Now);
            }
        }

        public int SelectedMemberTotalRentalsCount
        {
            get
            {
                if (SelectedMemberRentalHistory == null)
                {
                    return 0;
                }

                return SelectedMemberRentalHistory.Count;
            }
        }

        #endregion

        #region Commands

        public ICommand SelectBookCommand { get; private set; }
        public ICommand DeleteBookCommand { get; private set; }
        public ICommand RentBookCommand { get; private set; }
        public ICommand ReturnBookCommand { get; private set; }
        public ICommand SearchMembersCommand { get; private set; }
        public ICommand RefreshDataCommand { get; private set; }
        public ICommand AddBookCommand { get; private set; }
        public ICommand EditBookCommand { get; private set; }
        public ICommand SaveBookCommand { get; private set; }
        public ICommand CancelBookCommand { get; private set; }
        public ICommand ViewRentalHistoryCommand { get; private set; }
        public ICommand SearchMemberOverviewCommand { get; private set; }
        public ICommand ViewMemberOverviewCommand { get; private set; }

        #endregion

        #region Data loading

        private void LoadData()
        {
            LoadBooks();
            LoadActiveRentals();
            LoadRentalHistory();
        }

        private bool TryGetCurrentLibraryId(out int libraryId)
        {
            libraryId = 0;

            if (_selectedLibrary == null)
            {
                BookStatusMessage = "Library information not available.";
                RentalStatusMessage = "Library information not available.";
                return false;
            }

            libraryId = _selectedLibrary.Id;

            if (libraryId <= 0)
            {
                BookStatusMessage = "Invalid library selection.";
                RentalStatusMessage = "Invalid library selection.";
                return false;
            }

            return true;
        }

        private void LoadBooks()
        {
            try
            {
                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    Books = new ObservableCollection<Model_Book>();
                    FilteredBooks = new ObservableCollection<Model_Book>();
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var books = db.Books
                        .Include(book => book.Library)
                        .Where(book => book.IsActive && book.LibraryId == libraryId)
                        .OrderBy(book => book.Title)
                        .ToList();

                    Books = new ObservableCollection<Model_Book>(books);
                }

                if (Books.Count == 0)
                {
                    BookStatusMessage = "No books have been added to this library yet.";
                }
                else
                {
                    BookStatusMessage = string.Format(
                        "{0} books loaded successfully. {1} available for rent.",
                        Books.Count,
                        AvailableBooksCount);
                }

                OnPropertyChanged(nameof(TotalBooks));
                OnPropertyChanged(nameof(AvailableBooksCount));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                BookStatusMessage = "Books could not be loaded.";

                MessageBox.Show(
                    "Error loading books: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadActiveRentals()
        {
            try
            {
                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    ActiveRentals = new ObservableCollection<Model_Rental>();
                    RentalStatusMessage = "No library selected.";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var rentals = db.Rentals
                        .Include(rental => rental.Book)
                        .Include(rental => rental.Member)
                        .Where(rental => !rental.ReturnDate.HasValue && rental.Book.LibraryId == libraryId)
                        .OrderBy(rental => rental.DueDate)
                        .ToList();

                    ActiveRentals = new ObservableCollection<Model_Rental>(rentals);
                }

                if (ActiveRentals.Count == 0)
                {
                    RentalStatusMessage = "No active rentals at the moment.";
                }
                else
                {
                    int overdueCount = ActiveRentals.Count(rental => rental.DueDate < DateTime.Now);

                    RentalStatusMessage = string.Format(
                        "{0} active rental(s) found. {1} overdue.",
                        ActiveRentals.Count,
                        overdueCount);
                }

                OnPropertyChanged(nameof(ActiveRentalsCount));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                ActiveRentals = new ObservableCollection<Model_Rental>();
                RentalStatusMessage = "Active rentals could not be loaded.";

                MessageBox.Show(
                    "Error loading rentals: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadRentalHistory()
        {
            try
            {
                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    RentalHistory = new ObservableCollection<Model_Rental>();
                    RentalStatusMessage = "No library selected.";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var history = db.Rentals
                        .Include(rental => rental.Book)
                        .Include(rental => rental.Member)
                        .Where(rental => rental.Book.LibraryId == libraryId)
                        .OrderByDescending(rental => rental.RentalDate)
                        .Take(100)
                        .ToList();

                    RentalHistory = new ObservableCollection<Model_Rental>(history);
                }

                if (RentalHistory.Count == 0)
                {
                    RentalStatusMessage = "No rental history found.";
                }
                else
                {
                    RentalStatusMessage = string.Format(
                        "{0} rental history record(s) loaded.",
                        RentalHistory.Count);
                }
            }
            catch (Exception ex)
            {
                RentalHistory = new ObservableCollection<Model_Rental>();
                RentalStatusMessage = "Rental history could not be loaded.";

                MessageBox.Show(
                    "Error loading rental history: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Book filtering

        private void FilterBooks()
        {
            IEnumerable<Model_Book> filtered;

            if (Books == null)
            {
                filtered = new List<Model_Book>();
            }
            else
            {
                filtered = Books.AsEnumerable();
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.Trim().ToLower();

                filtered = filtered.Where(book =>
                    (book.Title != null && book.Title.ToLower().Contains(searchLower)) ||
                    (book.Author != null && book.Author.ToLower().Contains(searchLower)));
            }

            FilteredBooks = new ObservableCollection<Model_Book>(filtered);
        }

        #endregion

        #region Book form helpers

        private Model_Book CreateEmptyBookForm()
        {
            return new Model_Book
            {
                Title = string.Empty,
                Author = string.Empty,
                Quantity = 1,
                AvailableQuantity = 1,
                IsActive = true
            };
        }

        private void ClearBookForm()
        {
            EditingBook = CreateEmptyBookForm();
            SelectedBook = null;
            IsBookFormReadOnly = false;

            BookStatusMessage = "Book form cleared.";
            CommandManager.InvalidateRequerySuggested();
        }

        private void SelectBook(Model_Book bookFromCommand)
        {
            if (bookFromCommand == null)
            {
                BookStatusMessage = "Select a book.";
                return;
            }

            SelectedBook = bookFromCommand;

            EditingBook = new Model_Book
            {
                Id = bookFromCommand.Id,
                Title = bookFromCommand.Title,
                Author = bookFromCommand.Author,
                LibraryId = bookFromCommand.LibraryId,
                Library = bookFromCommand.Library,
                Quantity = bookFromCommand.Quantity,
                AvailableQuantity = bookFromCommand.AvailableQuantity,
                IsActive = bookFromCommand.IsActive
            };

            IsBookFormReadOnly = true;

            BookStatusMessage = "Selected book for rental: " + bookFromCommand.Title;
            CommandManager.InvalidateRequerySuggested();
        }

        private void LoadBookIntoForm(Model_Book bookFromCommand)
        {
            if (bookFromCommand == null)
            {
                BookStatusMessage = "Select a book to edit.";

                MessageBox.Show(
                    "Please select a book to edit.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            SelectedBook = bookFromCommand;

            EditingBook = new Model_Book
            {
                Id = bookFromCommand.Id,
                Title = bookFromCommand.Title,
                Author = bookFromCommand.Author,
                LibraryId = bookFromCommand.LibraryId,
                Library = bookFromCommand.Library,
                Quantity = bookFromCommand.Quantity,
                AvailableQuantity = bookFromCommand.AvailableQuantity,
                IsActive = bookFromCommand.IsActive
            };

            IsBookFormReadOnly = false;

            BookStatusMessage = "Editing book: " + bookFromCommand.Title;
            CommandManager.InvalidateRequerySuggested();
        }

        private bool ValidateBook(Model_Book book)
        {
            if (book == null)
            {
                MessageBox.Show(
                    "Book information is missing.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                MessageBox.Show(
                    "Book title is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (book.Title.Trim().Length > 200)
            {
                MessageBox.Show(
                    "Book title cannot be longer than 200 characters.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (string.IsNullOrWhiteSpace(book.Author))
            {
                MessageBox.Show(
                    "Book author is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (book.Author.Trim().Length > 200)
            {
                MessageBox.Show(
                    "Book author cannot be longer than 200 characters.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (book.Quantity <= 0)
            {
                MessageBox.Show(
                    "Quantity must be at least 1.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (book.AvailableQuantity < 0)
            {
                MessageBox.Show(
                    "Available quantity cannot be negative.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            if (book.AvailableQuantity > book.Quantity)
            {
                MessageBox.Show(
                    "Available quantity cannot exceed total quantity.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        #endregion

        #region Book CRUD

        private void SaveBookFromForm()
        {
            try
            {
                if (IsBookFormReadOnly)
                {
                    BookStatusMessage = "Press EDIT before changing book details.";

                    MessageBox.Show(
                        "Press EDIT before changing book details.",
                        "Edit Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    MessageBox.Show(
                        BookStatusMessage,
                        "Library Selection Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (!ValidateBook(EditingBook))
                {
                    return;
                }

                if (EditingBook.Id <= 0)
                {
                    AddBookFromForm(libraryId);
                }
                else
                {
                    UpdateBookFromForm(libraryId);
                }
            }
            catch (Exception ex)
            {
                BookStatusMessage = "Book could not be saved.";

                MessageBox.Show(
                    "Error saving book: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AddBookFromForm(int libraryId)
        {
            using (var db = new LibraryDbContext())
            {
                var book = new Model_Book
                {
                    Title = EditingBook.Title.Trim(),
                    Author = EditingBook.Author.Trim(),
                    LibraryId = libraryId,
                    Quantity = EditingBook.Quantity,
                    AvailableQuantity = EditingBook.Quantity,
                    IsActive = true
                };

                db.Books.Add(book);
                db.SaveChanges();
            }

            BookStatusMessage = "Book added successfully.";

            MessageBox.Show(
                "Book added successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            ClearBookForm();
            RefreshDataAfterChange();
        }

        private void UpdateBookFromForm(int libraryId)
        {
            using (var db = new LibraryDbContext())
            {
                var book = db.Books.FirstOrDefault(item =>
                    item.Id == EditingBook.Id &&
                    item.LibraryId == libraryId &&
                    item.IsActive);

                if (book == null)
                {
                    BookStatusMessage = "Book not found or access denied.";

                    MessageBox.Show(
                        "Book not found or access denied.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                int activeRentalsCount = db.Rentals.Count(rental =>
                    rental.BookId == book.Id &&
                    !rental.ReturnDate.HasValue);

                if (EditingBook.Quantity < activeRentalsCount)
                {
                    BookStatusMessage = "Total quantity cannot be lower than currently rented copies.";

                    MessageBox.Show(
                        "Total quantity cannot be lower than currently rented copies.\nCurrently rented copies: " + activeRentalsCount,
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                book.Title = EditingBook.Title.Trim();
                book.Author = EditingBook.Author.Trim();
                book.Quantity = EditingBook.Quantity;
                book.AvailableQuantity = EditingBook.Quantity - activeRentalsCount;

                db.SaveChanges();
            }

            BookStatusMessage = "Book updated successfully.";

            MessageBox.Show(
                "Book updated successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            ClearBookForm();
            RefreshDataAfterChange();
        }

        private void DeleteBook()
        {
            if (SelectedBook == null)
            {
                BookStatusMessage = "Select a book to delete.";

                MessageBox.Show(
                    "Please select a book to delete.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string selectedBookTitle = SelectedBook.Title;
            int selectedBookId = SelectedBook.Id;

            var confirmation = MessageBox.Show(
                string.Format("Are you sure you want to delete \"{0}\"?\nThis action will hide the book from active lists.", selectedBookTitle),
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    BookStatusMessage = "Cannot delete book: no library selected.";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var book = db.Books.FirstOrDefault(item =>
                        item.Id == selectedBookId &&
                        item.LibraryId == libraryId &&
                        item.IsActive);

                    if (book == null)
                    {
                        BookStatusMessage = "The selected book no longer exists in this library.";
                        return;
                    }

                    bool hasActiveRentals = db.Rentals.Any(rental =>
                        rental.BookId == book.Id &&
                        !rental.ReturnDate.HasValue);

                    if (hasActiveRentals)
                    {
                        BookStatusMessage = "Cannot delete a book that has active rentals.";

                        MessageBox.Show(
                            "This book cannot be deleted because it has active rentals.",
                            "Cannot Delete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    book.IsActive = false;
                    db.SaveChanges();
                }

                RefreshDataAfterChange();

                BookStatusMessage = string.Format("Book \"{0}\" has been deleted.", selectedBookTitle);
                SelectedBook = null;
                EditingBook = CreateEmptyBookForm();
                IsBookFormReadOnly = false;
            }
            catch (Exception ex)
            {
                BookStatusMessage = "Book could not be deleted.";

                MessageBox.Show(
                    "Error deleting book: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Rental operations

        private void RentBook()
        {
            if (SelectedBook == null)
            {
                RentalStatusMessage = "Select a book to rent.";

                MessageBox.Show(
                    "Please select a book first.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string fullName = NormalizeText(RentMemberFullName);
            string email = NormalizeText(RentMemberEmail);
            string phone = NormalizeText(RentMemberPhone);

            if (string.IsNullOrWhiteSpace(fullName))
            {
                RentalStatusMessage = "Full name is required.";

                MessageBox.Show(
                    "Full name is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (fullName.Length > 200)
            {
                RentalStatusMessage = "Full name is too long.";

                MessageBox.Show(
                    "Full name cannot be longer than 200 characters.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (email.Length > 100)
            {
                RentalStatusMessage = "Email is too long.";

                MessageBox.Show(
                    "Email cannot be longer than 100 characters.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmailFormat(email))
            {
                RentalStatusMessage = "Email format is invalid.";

                MessageBox.Show(
                    "Email format is invalid.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (phone.Length > 20)
            {
                RentalStatusMessage = "Phone is too long.";

                MessageBox.Show(
                    "Phone cannot be longer than 20 characters.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            {
                RentalStatusMessage = "Email or phone is required.";

                MessageBox.Show(
                    "Please enter at least an email or a phone number so the member can be identified automatically.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!RentalDueDate.HasValue)
            {
                RentalStatusMessage = "Due date is required.";

                MessageBox.Show(
                    "Due date is required.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (RentalDueDate.Value <= DateTime.Now)
            {
                RentalStatusMessage = "The due date must be in the future.";

                MessageBox.Show(
                    "The due date must be in the future.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var confirmation = MessageBox.Show(
                string.Format(
                    "Rent \"{0}\" to {1}?\nDue date: {2:MM/dd/yyyy}",
                    SelectedBook.Title,
                    fullName,
                    RentalDueDate.Value),
                "Confirm Rental",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    RentalStatusMessage = "Cannot rent book: no library selected.";

                    MessageBox.Show(
                        RentalStatusMessage,
                        "Library Selection Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                string rentedBookTitle;
                string rentedMemberName;

                using (var db = new LibraryDbContext())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var book = db.Books.FirstOrDefault(item =>
                                item.Id == SelectedBook.Id &&
                                item.LibraryId == libraryId &&
                                item.IsActive);

                            if (book == null)
                            {
                                RentalStatusMessage = "The selected book no longer exists in this library.";
                                transaction.Rollback();
                                return;
                            }

                            if (book.AvailableQuantity <= 0)
                            {
                                RentalStatusMessage = "This book is not available for rent.";

                                MessageBox.Show(
                                    "This book has no available copies.",
                                    "Not Available",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                transaction.Rollback();
                                return;
                            }

                            Model_Member member = null;

                            if (!string.IsNullOrWhiteSpace(email))
                            {
                                member = db.Members.FirstOrDefault(item =>
                                    item.Email != null &&
                                    item.Email.ToLower() == email.ToLower());
                            }

                            if (member == null && !string.IsNullOrWhiteSpace(phone))
                            {
                                member = db.Members.FirstOrDefault(item =>
                                    item.Phone != null &&
                                    item.Phone == phone);
                            }

                            if (member == null)
                            {
                                string emailToSave;

                                if (string.IsNullOrWhiteSpace(email))
                                {
                                    emailToSave = null;
                                }
                                else
                                {
                                    emailToSave = email;
                                }

                                string phoneToSave;

                                if (string.IsNullOrWhiteSpace(phone))
                                {
                                    phoneToSave = null;
                                }
                                else
                                {
                                    phoneToSave = phone;
                                }

                                member = new Model_Member
                                {
                                    StudentId = GenerateUniqueAutoStudentId(db),
                                    FullName = fullName,
                                    Email = emailToSave,
                                    Phone = phoneToSave,
                                    Department = null,
                                    IsActive = true
                                };

                                db.Members.Add(member);
                                db.SaveChanges();
                            }
                            else
                            {
                                if (!member.IsActive)
                                {
                                    member.IsActive = true;
                                }

                                string existingName = NormalizeForSearch(member.FullName);
                                string newName = NormalizeForSearch(fullName);

                                if (!string.IsNullOrWhiteSpace(existingName) &&
                                    existingName != newName)
                                {
                                    RentalStatusMessage = "This email or phone already belongs to another member.";

                                    MessageBox.Show(
                                        "This email or phone already belongs to another member:\n\n" +
                                        member.FullName +
                                        "\n\nYou tried to rent the book to:\n\n" +
                                        fullName +
                                        "\n\nUse a different email or phone.",
                                        "Member Conflict",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);

                                    transaction.Rollback();
                                    return;
                                }

                                if (string.IsNullOrWhiteSpace(member.FullName))
                                {
                                    member.FullName = fullName;
                                }

                                if (!string.IsNullOrWhiteSpace(email) &&
                                    string.IsNullOrWhiteSpace(member.Email))
                                {
                                    member.Email = email;
                                }

                                if (!string.IsNullOrWhiteSpace(phone) &&
                                    string.IsNullOrWhiteSpace(member.Phone))
                                {
                                    member.Phone = phone;
                                }
                            }

                            rentedBookTitle = book.Title;
                            rentedMemberName = member.FullName;

                            book.AvailableQuantity--;

                            var rental = new Model_Rental
                            {
                                BookId = book.Id,
                                BookTitle = book.Title,
                                MemberId = member.Id,
                                MemberName = member.FullName,
                                StudentId = member.StudentId,
                                RentalDate = DateTime.Now,
                                DueDate = RentalDueDate.Value
                            };

                            db.Rentals.Add(rental);
                            db.SaveChanges();
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                RefreshDataAfterChange();

                RentalStatusMessage = string.Format(
                    "Book \"{0}\" rented to {1}. Due date: {2:MM/dd/yyyy}",
                    rentedBookTitle,
                    rentedMemberName,
                    RentalDueDate.Value);

                MessageBox.Show(
                    string.Format(
                        "Successfully rented \"{0}\" to {1}.\nPlease return by {2:MM/dd/yyyy}.",
                        rentedBookTitle,
                        rentedMemberName,
                        RentalDueDate.Value),
                    "Rental Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                SelectedBook = null;
                EditingBook = CreateEmptyBookForm();
                IsBookFormReadOnly = false;

                ClearRentMemberForm();
                RentalDueDate = DateTime.Now.AddDays(14);

                BookStatusMessage = "Book form cleared after rental.";
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Book could not be rented.";

                MessageBox.Show(
                    "Error renting book: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ReturnBook(Model_Rental rentalFromCommand)
        {
            Model_Rental rentalToReturn = rentalFromCommand;

            if (rentalToReturn == null)
            {
                rentalToReturn = SelectedRental;
            }

            if (rentalToReturn == null)
            {
                RentalStatusMessage = "Select a rental to return.";

                MessageBox.Show(
                    "Please select a rental to return.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (rentalToReturn.ReturnDate.HasValue)
            {
                RentalStatusMessage = "This rental has already been returned.";

                MessageBox.Show(
                    "This rental has already been returned.",
                    "Already Returned",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SelectedRental = rentalToReturn;

            string selectedBookTitle = rentalToReturn.BookTitle;
            string selectedMemberName = rentalToReturn.MemberName;
            bool isOverdue = rentalToReturn.DueDate < DateTime.Now;

            string confirmationMessage = string.Format(
                "Confirm return of \"{0}\" rented by {1}.",
                selectedBookTitle,
                selectedMemberName);

            if (isOverdue)
            {
                confirmationMessage += "\n\nWARNING: This rental is overdue!";
            }

            var confirmation = MessageBox.Show(
                confirmationMessage,
                "Confirm Return",
                MessageBoxButton.YesNo,
                isOverdue ? MessageBoxImage.Warning : MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    RentalStatusMessage = "Cannot return book: no library selected.";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var rental = db.Rentals.FirstOrDefault(item =>
                                item.Id == rentalToReturn.Id &&
                                !item.ReturnDate.HasValue);

                            if (rental == null)
                            {
                                RentalStatusMessage = "The selected rental no longer exists or was already returned.";
                                transaction.Rollback();
                                return;
                            }

                            var book = db.Books.FirstOrDefault(item =>
                                item.Id == rental.BookId &&
                                item.LibraryId == libraryId &&
                                item.IsActive);

                            if (book == null)
                            {
                                RentalStatusMessage = "The rented book no longer exists in this library.";
                                transaction.Rollback();
                                return;
                            }

                            if (book.AvailableQuantity >= book.Quantity)
                            {
                                RentalStatusMessage = "Book stock is already complete. Return cannot increase available quantity.";

                                MessageBox.Show(
                                    "Book stock is already complete. Return cannot increase available quantity.",
                                    "Validation Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                transaction.Rollback();
                                return;
                            }

                            rental.ReturnDate = DateTime.Now;
                            book.AvailableQuantity++;

                            db.SaveChanges();
                            transaction.Commit();

                            string returnMessage = string.Format(
                                "Successfully returned \"{0}\" rented by {1}.",
                                selectedBookTitle,
                                selectedMemberName);

                            if (isOverdue)
                            {
                                returnMessage += "\n\nNote: This rental was overdue.";
                            }

                            RentalStatusMessage = returnMessage;

                            MessageBox.Show(
                                returnMessage,
                                "Return Successful",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                RefreshDataAfterChange();

                SelectedRental = null;
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Book could not be returned.";

                MessageBox.Show(
                    "Error returning book: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Refresh and general helper methods

        private void RefreshDataAfterChange()
        {
            LoadBooks();
            LoadActiveRentals();
            LoadRentalHistory();

            if (SelectedOverviewMember != null)
            {
                LoadSelectedMemberRentals(SelectedOverviewMember.Id);
            }
        }

        private string GenerateUniqueAutoStudentId(LibraryDbContext db)
        {
            string generatedId;

            do
            {
                generatedId = "AUTO-" + Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper();
            }
            while (db.Members.Any(member => member.StudentId == generatedId));

            return generatedId;
        }

        private void SearchMembers()
        {
            try
            {
                using (var db = new LibraryDbContext())
                {
                    var query = db.Members
                        .Where(member => member.IsActive);

                    if (!string.IsNullOrWhiteSpace(MemberSearchText))
                    {
                        string searchLower = MemberSearchText.Trim().ToLower();

                        query = query.Where(member =>
                            (member.FullName != null && member.FullName.ToLower().Contains(searchLower)) ||
                            (member.StudentId != null && member.StudentId.ToLower().Contains(searchLower)) ||
                            (member.Email != null && member.Email.ToLower().Contains(searchLower)));
                    }

                    var members = query
                        .OrderBy(member => member.FullName)
                        .ToList();

                    Members = new ObservableCollection<Model_Member>(members);

                    if (members.Count == 0 && !string.IsNullOrWhiteSpace(MemberSearchText))
                    {
                        RentalStatusMessage = "No members found matching your search.";
                    }
                    else
                    {
                        RentalStatusMessage = string.Format("{0} member(s) loaded.", members.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                Members = new ObservableCollection<Model_Member>();
                RentalStatusMessage = "Error searching members.";

                MessageBox.Show(
                    "Error searching members: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ToggleRentalHistory()
        {
            ShowRentalHistory = !ShowRentalHistory;
        }

        private static bool IsValidEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ClearRentMemberForm()
        {
            RentMemberStudentId = string.Empty;
            RentMemberFullName = string.Empty;
            RentMemberEmail = string.Empty;
            RentMemberPhone = string.Empty;
        }

        private static string NormalizeText(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Trim();
        }

        #endregion

        #region Member overview operations

        private void SearchMemberOverview()
        {
            try
            {
                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    RentalStatusMessage = "Cannot search members: no library selected.";
                    return;
                }

                string search = NormalizeText(MemberOverviewSearchText);

                if (string.IsNullOrWhiteSpace(search))
                {
                    RentalStatusMessage = "Enter member name, email, phone or member code.";
                    MemberOverviewResults = new ObservableCollection<Model_Member>();
                    ClearSelectedMemberOverview();
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var rentalsFromThisLibrary = db.Rentals
                        .Include(rental => rental.Book)
                        .Include(rental => rental.Member)
                        .Where(rental =>
                            rental.Book.LibraryId == libraryId &&
                            rental.Member != null &&
                            rental.Member.IsActive)
                        .ToList();

                    string normalizedSearch = NormalizeForSearch(search);

                    string[] tokens = normalizedSearch.Split(
                        new char[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);

                    var members = new List<Model_Member>();
                    var usedMemberIds = new HashSet<int>();

                    foreach (var rental in rentalsFromThisLibrary)
                    {
                        string searchableText = BuildMemberSearchTextFromRental(rental);

                        bool matchesAllTokens = true;

                        foreach (string token in tokens)
                        {
                            if (!searchableText.Contains(token))
                            {
                                matchesAllTokens = false;
                                break;
                            }
                        }

                        if (matchesAllTokens)
                        {
                            if (!usedMemberIds.Contains(rental.MemberId))
                            {
                                usedMemberIds.Add(rental.MemberId);
                                members.Add(rental.Member);
                            }
                        }
                    }

                    members = members
                        .OrderBy(member => member.FullName)
                        .ToList();

                    MemberOverviewResults = new ObservableCollection<Model_Member>(members);

                    if (members.Count == 0)
                    {
                        ClearSelectedMemberOverview();
                        RentalStatusMessage = "No member matched this search in this library rentals.";
                    }
                    else if (members.Count == 1)
                    {
                        ViewMemberOverview(members[0]);
                        RentalStatusMessage = "1 member found. Member overview loaded automatically.";
                    }
                    else
                    {
                        ClearSelectedMemberOverview();
                        RentalStatusMessage = members.Count + " member(s) found. Press VIEW for the correct member.";
                    }
                }
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Member search failed.";

                MessageBox.Show(
                    "Error searching members: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ClearSelectedMemberOverview()
        {
            SelectedOverviewMember = null;
            SelectedMemberActiveRentals = new ObservableCollection<Model_Rental>();
            SelectedMemberRentalHistory = new ObservableCollection<Model_Rental>();

            OnPropertyChanged(nameof(SelectedOverviewMemberName));
            OnPropertyChanged(nameof(SelectedOverviewMemberEmail));
            OnPropertyChanged(nameof(SelectedOverviewMemberPhone));
            OnPropertyChanged(nameof(SelectedOverviewMemberCode));
            OnPropertyChanged(nameof(SelectedMemberActiveRentalsCount));
            OnPropertyChanged(nameof(SelectedMemberOverdueRentalsCount));
            OnPropertyChanged(nameof(SelectedMemberTotalRentalsCount));
        }

        private static string BuildMemberSearchTextFromRental(Model_Rental rental)
        {
            var builder = new StringBuilder();

            if (rental == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(rental.MemberName))
            {
                builder.Append(rental.MemberName);
                builder.Append(" ");
            }

            if (!string.IsNullOrWhiteSpace(rental.StudentId))
            {
                builder.Append(rental.StudentId);
                builder.Append(" ");
            }

            if (rental.Member != null)
            {
                if (!string.IsNullOrWhiteSpace(rental.Member.FullName))
                {
                    builder.Append(rental.Member.FullName);
                    builder.Append(" ");
                }

                if (!string.IsNullOrWhiteSpace(rental.Member.Email))
                {
                    builder.Append(rental.Member.Email);
                    builder.Append(" ");
                }

                if (!string.IsNullOrWhiteSpace(rental.Member.Phone))
                {
                    builder.Append(rental.Member.Phone);
                    builder.Append(" ");
                }

                if (!string.IsNullOrWhiteSpace(rental.Member.StudentId))
                {
                    builder.Append(rental.Member.StudentId);
                    builder.Append(" ");
                }
            }

            return NormalizeForSearch(builder.ToString());
        }

        private static string NormalizeForSearch(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string lowerValue = value.ToLower().Trim();
            string normalizedValue = lowerValue.Normalize(NormalizationForm.FormD);

            var builder = new StringBuilder();

            foreach (char character in normalizedValue)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private void ViewMemberOverview(Model_Member member)
        {
            if (member == null)
            {
                RentalStatusMessage = "Select a member.";
                return;
            }

            SelectedOverviewMember = member;
            LoadSelectedMemberRentals(member.Id);
        }

        private void LoadSelectedMemberRentals(int memberId)
        {
            try
            {
                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    RentalStatusMessage = "Cannot load member rentals: no library selected.";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var activeRentals = db.Rentals
                        .Include(rental => rental.Book)
                        .Where(rental =>
                            rental.MemberId == memberId &&
                            rental.Book.LibraryId == libraryId &&
                            !rental.ReturnDate.HasValue)
                        .OrderBy(rental => rental.DueDate)
                        .ToList();

                    var history = db.Rentals
                        .Include(rental => rental.Book)
                        .Where(rental =>
                            rental.MemberId == memberId &&
                            rental.Book.LibraryId == libraryId)
                        .OrderByDescending(rental => rental.RentalDate)
                        .ToList();

                    SelectedMemberActiveRentals = new ObservableCollection<Model_Rental>(activeRentals);
                    SelectedMemberRentalHistory = new ObservableCollection<Model_Rental>(history);
                }

                OnPropertyChanged(nameof(SelectedMemberActiveRentalsCount));
                OnPropertyChanged(nameof(SelectedMemberOverdueRentalsCount));
                OnPropertyChanged(nameof(SelectedMemberTotalRentalsCount));

                RentalStatusMessage = "Member overview loaded.";
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Member overview could not be loaded.";

                MessageBox.Show(
                    "Error loading member overview: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
