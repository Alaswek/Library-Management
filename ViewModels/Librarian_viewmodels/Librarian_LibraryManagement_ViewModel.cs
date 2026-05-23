using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LibraryManagement.ViewModels
{
    public class Librarian_LibraryManagement_ViewModel : ViewModelBase
    {
        private readonly Model_User _currentLibrarian;

        private ObservableCollection<Model_Book> _books;
        private ObservableCollection<Model_Book> _filteredBooks;
        private ObservableCollection<Model_Rental> _activeRentals;
        private ObservableCollection<Model_Rental> _rentalHistory;
        private ObservableCollection<Model_Member> _members;
        private ObservableCollection<string> _categories;

        private string _searchText;
        private string _memberSearchText;
        private string _selectedCategory;

        private Model_Book _selectedBook;
        private Model_Member _selectedMember;
        private Model_Rental _selectedRental;

        private DateTime _rentalDueDate;
        private string _bookStatusMessage;
        private string _rentalStatusMessage;
        private bool _showRentalHistory;

        // Proprietăți pentru gestionarea cărților (CRUD)
        private Model_Book _newBook;
        private Model_Book _editingBook;
        private bool _isAddingBook;
        private bool _isEditingBook;

        public Librarian_LibraryManagement_ViewModel()
            : this(null)
        {
        }

        public Librarian_LibraryManagement_ViewModel(Model_User currentLibrarian)
        {
            _currentLibrarian = currentLibrarian;
            _rentalDueDate = DateTime.Now.AddDays(14);
            _showRentalHistory = false;

            _books = new ObservableCollection<Model_Book>();
            _filteredBooks = new ObservableCollection<Model_Book>();
            _activeRentals = new ObservableCollection<Model_Rental>();
            _rentalHistory = new ObservableCollection<Model_Rental>();
            _members = new ObservableCollection<Model_Member>();
            _categories = new ObservableCollection<string>();
            _newBook = new Model_Book();
            _editingBook = new Model_Book();

            // Inițializare comenzi
            DeleteBookCommand = new RelayCommand(_ => DeleteBook(), _ => CanDeleteBook());
            RentBookCommand = new RelayCommand(_ => RentBook(), _ => CanRentBook());
            ReturnBookCommand = new RelayCommand(_ => ReturnBook(), _ => CanReturnBook());
            SearchMembersCommand = new RelayCommand(_ => SearchMembers());
            RefreshDataCommand = new RelayCommand(_ => LoadData());

            // Noi comenzi pentru gestionarea completă a cărților
            AddBookCommand = new RelayCommand(_ => ShowAddBookDialog(), _ => CanAddBook());
            EditBookCommand = new RelayCommand(_ => ShowEditBookDialog(), _ => CanEditBook());
            SaveBookCommand = new RelayCommand(_ => SaveBook(), _ => CanSaveBook());
            CancelBookCommand = new RelayCommand(_ => CancelBookOperation());
            ViewRentalHistoryCommand = new RelayCommand(_ => ToggleRentalHistory());

            LoadData();
        }

        // Proprietăți publice
        public string WelcomeMessage
        {
            get
            {
                if (_currentLibrarian != null && !string.IsNullOrWhiteSpace(_currentLibrarian.Username))
                {
                    return string.Format("Welcome, {0} - Library Management System", _currentLibrarian.Username);
                }

                return "Welcome, Librarian - Library Management System";
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
                    if (_selectedBook != null && !_isEditingBook)
                    {
                        LoadBookForEditing();
                    }
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

        public DateTime RentalDueDate
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

        // Proprietăți pentru gestionarea cărților
        public Model_Book NewBook
        {
            get { return _newBook; }
            set { SetProperty(ref _newBook, value); }
        }

        public Model_Book EditingBook
        {
            get { return _editingBook; }
            set { SetProperty(ref _editingBook, value); }
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

        // Comenzi publice
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

        // Metode private pentru încărcare date
        private void LoadData()
        {
            LoadBooks();
            LoadCategories();
            LoadActiveRentals();
            LoadMembers();
            if (_showRentalHistory)
            {
                LoadRentalHistory();
            }
        }

        private bool TryGetCurrentLibraryId(out int libraryId)
        {
            libraryId = 0;

            if (_currentLibrarian == null)
            {
                BookStatusMessage = "Librarian information not available.";
                return false;
            }

            if (!_currentLibrarian.Library_ID.HasValue)
            {
                BookStatusMessage = "This librarian is not assigned to a library. Please contact an administrator.";
                return false;
            }

            libraryId = _currentLibrarian.Library_ID.Value;

            if (libraryId <= 0)
            {
                BookStatusMessage = "Invalid library assignment.";
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
                    BookStatusMessage = "No books have been added to your library yet. Use 'Add Book' to get started.";
                }
                else
                {
                    BookStatusMessage = string.Format("{0} books loaded successfully. {1} available for rent.",
                        Books.Count, AvailableBooksCount);
                }

                OnPropertyChanged(nameof(TotalBooks));
                OnPropertyChanged(nameof(AvailableBooksCount));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                BookStatusMessage = "Books could not be loaded.";
                MessageBox.Show("Error loading books: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                Categories.Clear();
                Categories.Add("All Categories");

                int libraryId;

                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    SelectedCategory = "All Categories";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var library = db.Libraries.FirstOrDefault(item => item.Id == libraryId && item.IsOpen);

                    if (library != null)
                    {
                        Categories.Add(library.Name);
                    }
                }

                SelectedCategory = "All Categories";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading categories: " + ex.Message);
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
                    RentalStatusMessage = "This librarian is not assigned to a library.";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var rentals = db.Rentals
                        .Include(r => r.Book)
                        .Include(r => r.Member)
                        .Where(r => !r.ReturnDate.HasValue && r.Book.LibraryId == libraryId)
                        .OrderBy(r => r.DueDate)
                        .ToList();

                    ActiveRentals = new ObservableCollection<Model_Rental>(rentals);
                }

                if (ActiveRentals.Count == 0)
                {
                    RentalStatusMessage = "No active rentals at the moment.";
                }
                else
                {
                    int overdueCount = ActiveRentals.Count(r => r.DueDate < DateTime.Now);
                    RentalStatusMessage = string.Format("{0} active rental(s) found. {1} overdue.",
                        ActiveRentals.Count, overdueCount);
                }

                OnPropertyChanged(nameof(ActiveRentalsCount));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Active rentals could not be loaded.";
                MessageBox.Show("Error loading rentals: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var history = db.Rentals
                        .Include(r => r.Book)
                        .Include(r => r.Member)
                        .Where(r => r.Book.LibraryId == libraryId)
                        .OrderByDescending(r => r.RentalDate)
                        .Take(100) // Limităm pentru performanță
                        .ToList();

                    RentalHistory = new ObservableCollection<Model_Rental>(history);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading rental history: " + ex.Message);
            }
        }

        private void LoadMembers()
        {
            try
            {
                using (var db = new LibraryDbContext())
                {
                    var members = db.Members
                        .Where(member => member.IsActive)
                        .OrderBy(member => member.FullName)
                        .ToList();

                    Members = new ObservableCollection<Model_Member>(members);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading members: " + ex.Message);
            }
        }

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

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All Categories")
            {
                filtered = filtered.Where(book =>
                    book.Library != null &&
                    book.Library.Name == SelectedCategory);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.Trim().ToLower();

                filtered = filtered.Where(book =>
                    (book.Title?.ToLower().Contains(searchLower) ?? false) ||
                    (book.Author?.ToLower().Contains(searchLower) ?? false));
            }

            FilteredBooks = new ObservableCollection<Model_Book>(filtered);
        }

        // Gestionarea cărților - CRUD complet
        private bool CanAddBook()
        {
            int libraryId;
            return TryGetCurrentLibraryId(out libraryId);
        }

        private void ShowAddBookDialog()
        {
            NewBook = new Model_Book
            {
                IsActive = true,
                Quantity = 1,
                AvailableQuantity = 1
            };

            IsAddingBook = true;

            // Creare dialog personalizat pentru adăugare carte
            var dialog = new Window
            {
                Title = "Add New Book",
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = CreateBookForm("Add New Book", NewBook, true)
            };

            dialog.ShowDialog();
        }

        private void ShowEditBookDialog()
        {
            if (SelectedBook == null)
            {
                MessageBox.Show("Please select a book to edit.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsEditingBook = true;
            LoadBookForEditing();

            var dialog = new Window
            {
                Title = "Edit Book",
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = CreateBookForm("Edit Book", EditingBook, false)
            };

            dialog.ShowDialog();
        }

        private FrameworkElement CreateBookForm(string title, Model_Book book, bool isNew)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(10) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Title
            stackPanel.Children.Add(new TextBlock { Text = "Title:", Margin = new Thickness(0, 5, 0, 2) });
            var titleBox = new TextBox { Text = book.Title, Margin = new Thickness(0, 0, 0, 10) };
            titleBox.TextChanged += (s, e) => book.Title = titleBox.Text;
            stackPanel.Children.Add(titleBox);

            // Author
            stackPanel.Children.Add(new TextBlock { Text = "Author:", Margin = new Thickness(0, 5, 0, 2) });
            var authorBox = new TextBox { Text = book.Author, Margin = new Thickness(0, 0, 0, 10) };
            authorBox.TextChanged += (s, e) => book.Author = authorBox.Text;
            stackPanel.Children.Add(authorBox);

            // Quantity
            stackPanel.Children.Add(new TextBlock { Text = "Quantity:", Margin = new Thickness(0, 5, 0, 2) });
            var quantityBox = new TextBox { Text = book.Quantity.ToString(), Margin = new Thickness(0, 0, 0, 10) };
            quantityBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(quantityBox.Text, out int qty))
                {
                    book.Quantity = qty;
                    if (!isNew && book.AvailableQuantity > qty)
                    {
                        book.AvailableQuantity = qty;
                    }
                }
            };
            stackPanel.Children.Add(quantityBox);

            if (!isNew)
            {
                // Available Quantity (only for edit, for new it's automatically equal to quantity)
                stackPanel.Children.Add(new TextBlock { Text = "Available Quantity:", Margin = new Thickness(0, 5, 0, 2) });
                var availableBox = new TextBox { Text = book.AvailableQuantity.ToString(), Margin = new Thickness(0, 0, 0, 10) };
                availableBox.TextChanged += (s, e) =>
                {
                    if (int.TryParse(availableBox.Text, out int avail))
                    {
                        book.AvailableQuantity = Math.Min(avail, book.Quantity);
                    }
                };
                stackPanel.Children.Add(availableBox);
            }

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };

            var saveButton = new Button
            {
                Content = "Save",
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White
            };
            saveButton.Click += (s, e) =>
            {
                if (ValidateBook(book))
                {
                    SaveBook();
                    (saveButton.Parent as Window)?.Close();
                }
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 30,
                Margin = new Thickness(5),
                Background = System.Windows.Media.Brushes.Gray,
                Foreground = System.Windows.Media.Brushes.White
            };
            cancelButton.Click += (s, e) =>
            {
                CancelBookOperation();
                (cancelButton.Parent as Window)?.Close();
            };

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            var scrollViewer = new ScrollViewer { Content = stackPanel };
            return scrollViewer;
        }

        private bool ValidateBook(Model_Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
            {
                MessageBox.Show("Book title is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(book.Author))
            {
                MessageBox.Show("Book author is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (book.Quantity < 0)
            {
                MessageBox.Show("Quantity cannot be negative.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (book.AvailableQuantity < 0)
            {
                MessageBox.Show("Available quantity cannot be negative.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (book.AvailableQuantity > book.Quantity)
            {
                MessageBox.Show("Available quantity cannot exceed total quantity.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool CanEditBook()
        {
            return SelectedBook != null && SelectedBook.IsActive;
        }

        private void LoadBookForEditing()
        {
            if (SelectedBook != null)
            {
                EditingBook = new Model_Book
                {
                    Id = SelectedBook.Id,
                    Title = SelectedBook.Title,
                    Author = SelectedBook.Author,
                    LibraryId = SelectedBook.LibraryId,
                    Quantity = SelectedBook.Quantity,
                    AvailableQuantity = SelectedBook.AvailableQuantity,
                    IsActive = SelectedBook.IsActive
                };
            }
        }

        private bool CanSaveBook()
        {
            return IsAddingBook || IsEditingBook;
        }

        private void SaveBook()
        {
            try
            {
                int libraryId;
                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    BookStatusMessage = "Cannot save book: Librarian not assigned to a library.";
                    return;
                }

                if (IsAddingBook)
                {
                    // Adăugare carte nouă
                    if (!ValidateBook(NewBook))
                        return;

                    using (var db = new LibraryDbContext())
                    {
                        var book = new Model_Book
                        {
                            Title = NewBook.Title.Trim(),
                            Author = NewBook.Author.Trim(),
                            LibraryId = libraryId,
                            Quantity = NewBook.Quantity,
                            AvailableQuantity = NewBook.Quantity, // Inițial, toate copiile sunt disponibile
                            IsActive = true
                        };

                        db.Books.Add(book);
                        db.SaveChanges();
                    }

                    BookStatusMessage = string.Format("Book \"{0}\" has been added successfully.", NewBook.Title);
                    MessageBox.Show("Book added successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (IsEditingBook && EditingBook != null)
                {
                    // Editare carte existentă
                    if (!ValidateBook(EditingBook))
                        return;

                    using (var db = new LibraryDbContext())
                    {
                        var book = db.Books.FirstOrDefault(b => b.Id == EditingBook.Id && b.LibraryId == libraryId);

                        if (book == null)
                        {
                            BookStatusMessage = "Book not found or access denied.";
                            return;
                        }

                        // Verificăm dacă există închirieri active
                        var hasActiveRentals = db.Rentals.Any(r => r.BookId == book.Id && !r.ReturnDate.HasValue);

                        if (hasActiveRentals && EditingBook.Quantity < book.Quantity)
                        {
                            var result = MessageBox.Show(
                                "This book has active rentals. Reducing quantity might affect existing rentals. Continue?",
                                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                            if (result != MessageBoxResult.Yes)
                                return;
                        }

                        // Actualizăm cartea
                        book.Title = EditingBook.Title.Trim();
                        book.Author = EditingBook.Author.Trim();

                        int oldQuantity = book.Quantity;
                        book.Quantity = EditingBook.Quantity;

                        // Ajustăm available quantity în funcție de schimbarea quantity
                        if (EditingBook.Quantity > oldQuantity)
                        {
                            // Am adăugat copii noi - toate disponibile
                            book.AvailableQuantity += (EditingBook.Quantity - oldQuantity);
                        }
                        else if (EditingBook.Quantity < oldQuantity)
                        {
                            // Am eliminat copii - asigurăm că available nu depășește quantity
                            book.AvailableQuantity = Math.Min(EditingBook.AvailableQuantity, EditingBook.Quantity);
                        }
                        else
                        {
                            // Quantity neschimbat, actualizăm doar available quantity
                            book.AvailableQuantity = EditingBook.AvailableQuantity;
                        }

                        db.SaveChanges();
                    }

                    BookStatusMessage = string.Format("Book \"{0}\" has been updated successfully.", EditingBook.Title);
                    MessageBox.Show("Book updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Reîncărcăm datele
                LoadBooks();
                LoadActiveRentals();
                CancelBookOperation();
            }
            catch (Exception ex)
            {
                BookStatusMessage = "Failed to save book.";
                MessageBox.Show("Error saving book: " + ex.Message, "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBookOperation()
        {
            IsAddingBook = false;
            IsEditingBook = false;
            NewBook = new Model_Book();
            EditingBook = new Model_Book();
        }

        private bool CanDeleteBook()
        {
            if (SelectedBook == null)
                return false;

            // Verificăm dacă nu există închirieri active
            return SelectedBook.AvailableQuantity == SelectedBook.Quantity && SelectedBook.IsActive;
        }

        private void DeleteBook()
        {
            if (SelectedBook == null)
            {
                BookStatusMessage = "Select a book to delete.";
                return;
            }

            // Verificări suplimentare pentru siguranță
            if (SelectedBook.AvailableQuantity != SelectedBook.Quantity)
            {
                BookStatusMessage = "Cannot delete a book that has active rentals.";
                MessageBox.Show("This book cannot be deleted because it has active rentals.\nPlease return all copies before deleting.",
                    "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedBookTitle = SelectedBook.Title;
            var selectedBookId = SelectedBook.Id;

            var confirmation = MessageBox.Show(
                string.Format("Are you sure you want to delete \"{0}\"?\nThis action cannot be undone.", selectedBookTitle),
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                int libraryId;
                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    BookStatusMessage = "Cannot delete book: Librarian not assigned to a library.";
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
                        BookStatusMessage = "The selected book no longer exists in your library.";
                        return;
                    }

                    // Verificare finală înainte de ștergere
                    var hasActiveRentals = db.Rentals.Any(rental =>
                        rental.BookId == book.Id &&
                        !rental.ReturnDate.HasValue);

                    if (hasActiveRentals)
                    {
                        BookStatusMessage = "Cannot delete a book that has active rentals.";
                        MessageBox.Show("This book cannot be deleted because it has active rentals.",
                            "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Ștergere logică (setăm IsActive = false)
                    book.IsActive = false;
                    db.SaveChanges();
                }

                LoadBooks();
                BookStatusMessage = string.Format("Book \"{0}\" has been deleted.", selectedBookTitle);
                SelectedBook = null;
            }
            catch (Exception ex)
            {
                BookStatusMessage = "Book could not be deleted.";
                MessageBox.Show("Error deleting book: " + ex.Message, "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Gestionarea închirierilor
        private bool CanRentBook()
        {
            return SelectedBook != null &&
                   SelectedMember != null &&
                   SelectedBook.AvailableQuantity > 0 &&
                   RentalDueDate > DateTime.Now &&
                   SelectedBook.IsActive;
        }

        private void RentBook()
        {
            if (SelectedBook == null)
            {
                RentalStatusMessage = "Select a book to rent.";
                return;
            }

            if (SelectedMember == null)
            {
                RentalStatusMessage = "Select a member to rent the book to.";
                return;
            }

            if (RentalDueDate <= DateTime.Now)
            {
                RentalStatusMessage = "The due date must be in the future.";
                return;
            }

            if (SelectedBook.AvailableQuantity <= 0)
            {
                RentalStatusMessage = "This book is not available for rent.";
                MessageBox.Show("This book has no available copies.", "Not Available",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show(
                string.Format("Rent \"{0}\" to {1} (Student ID: {2})?\nDue date: {3:MM/dd/yyyy}",
                    SelectedBook.Title, SelectedMember.FullName, SelectedMember.StudentId, RentalDueDate),
                "Confirm Rental",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
                return;

            try
            {
                int libraryId;
                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    RentalStatusMessage = "Cannot rent book: Librarian not assigned to a library.";
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
                                RentalStatusMessage = "The selected book no longer exists in your library.";
                                transaction.Rollback();
                                return;
                            }

                            if (book.AvailableQuantity <= 0)
                            {
                                RentalStatusMessage = "This book is not available for rent.";
                                transaction.Rollback();
                                return;
                            }

                            var member = db.Members.FirstOrDefault(item =>
                                item.Id == SelectedMember.Id &&
                                item.IsActive);

                            if (member == null)
                            {
                                RentalStatusMessage = "The selected member no longer exists or is inactive.";
                                transaction.Rollback();
                                return;
                            }

                            rentedBookTitle = book.Title;
                            rentedMemberName = member.FullName;

                            // Actualizăm cantitatea disponibilă
                            book.AvailableQuantity--;

                            // Creăm înregistrarea închirierii
                            var rental = new Model_Rental
                            {
                                BookId = book.Id,
                                BookTitle = book.Title,
                                MemberId = member.Id,
                                MemberName = member.FullName,
                                StudentId = member.StudentId,
                                RentalDate = DateTime.Now,
                                DueDate = RentalDueDate
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

                LoadBooks();
                LoadActiveRentals();
                if (_showRentalHistory)
                {
                    LoadRentalHistory();
                }

                RentalStatusMessage = string.Format("Book \"{0}\" rented to {1}. Due date: {2:MM/dd/yyyy}",
                    rentedBookTitle, rentedMemberName, RentalDueDate);

                MessageBox.Show(string.Format("Successfully rented \"{0}\" to {1}.\nPlease return by {2:MM/dd/yyyy}.",
                    rentedBookTitle, rentedMemberName, RentalDueDate),
                    "Rental Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                // Resetare selecții după închiriere
                SelectedBook = null;
                SelectedMember = null;
                RentalDueDate = DateTime.Now.AddDays(14);
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Book could not be rented.";
                MessageBox.Show("Error renting book: " + ex.Message, "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanReturnBook()
        {
            return SelectedRental != null && !SelectedRental.ReturnDate.HasValue;
        }

        private void ReturnBook()
        {
            if (SelectedRental == null)
            {
                RentalStatusMessage = "Select a rental to return.";
                return;
            }

            if (SelectedRental.ReturnDate.HasValue)
            {
                RentalStatusMessage = "This rental has already been returned.";
                return;
            }

            var selectedBookTitle = SelectedRental.BookTitle;
            var selectedMemberName = SelectedRental.MemberName;
            bool isOverdue = SelectedRental.DueDate < DateTime.Now;

            string confirmationMessage = string.Format("Confirm return of \"{0}\" rented by {1}.",
                selectedBookTitle, selectedMemberName);

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
                return;

            try
            {
                int libraryId;
                if (!TryGetCurrentLibraryId(out libraryId))
                {
                    RentalStatusMessage = "Cannot return book: Librarian not assigned to a library.";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var rental = db.Rentals.FirstOrDefault(item =>
                                item.Id == SelectedRental.Id &&
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
                                RentalStatusMessage = "The rented book no longer exists in your library.";
                                transaction.Rollback();
                                return;
                            }

                            // Returnăm cartea
                            rental.ReturnDate = DateTime.Now;
                            book.AvailableQuantity++;

                            db.SaveChanges();
                            transaction.Commit();

                            string returnMessage = string.Format("Successfully returned \"{0}\" rented by {1}.",
                                selectedBookTitle, selectedMemberName);

                            if (isOverdue)
                            {
                                returnMessage += "\n\nNote: This rental was overdue.";
                            }

                            RentalStatusMessage = returnMessage;
                            MessageBox.Show(returnMessage, "Return Successful",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                LoadBooks();
                LoadActiveRentals();
                if (_showRentalHistory)
                {
                    LoadRentalHistory();
                }

                SelectedRental = null;
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Book could not be returned.";
                MessageBox.Show("Error returning book: " + ex.Message, "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                        var searchLower = MemberSearchText.Trim().ToLower();

                        query = query.Where(member =>
                            member.FullName.ToLower().Contains(searchLower) ||
                            member.StudentId.ToLower().Contains(searchLower) ||
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error searching members: " + ex.Message);
                RentalStatusMessage = "Error searching members.";
            }
        }

        private void ToggleRentalHistory()
        {
            ShowRentalHistory = !ShowRentalHistory;
        }
    }
}