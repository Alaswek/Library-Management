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

        public Librarian_LibraryManagement_ViewModel()
            : this(null)
        {
        }

        public Librarian_LibraryManagement_ViewModel(Model_User currentLibrarian)
        {
            _currentLibrarian = currentLibrarian;
            _rentalDueDate = DateTime.Now.AddDays(14);

            _books = new ObservableCollection<Model_Book>();
            _filteredBooks = new ObservableCollection<Model_Book>();
            _activeRentals = new ObservableCollection<Model_Rental>();
            _members = new ObservableCollection<Model_Member>();
            _categories = new ObservableCollection<string>();

            DeleteBookCommand = new RelayCommand(_ => DeleteBook(), _ => CanDeleteBook());
            RentBookCommand = new RelayCommand(_ => RentBook(), _ => CanRentBook());
            ReturnBookCommand = new RelayCommand(_ => ReturnBook(), _ => CanReturnBook());
            SearchMembersCommand = new RelayCommand(_ => SearchMembers());
            RefreshDataCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        public string WelcomeMessage
        {
            get
            {
                if (_currentLibrarian != null && !string.IsNullOrWhiteSpace(_currentLibrarian.Username))
                {
                    return string.Format("Welcome, {0}", _currentLibrarian.Username);
                }

                return "Welcome, Librarian";
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

        public int TotalBooks
        {
            get
            {
                if (Books == null)
                {
                    return 0;
                }

                return Books.Count;
            }
        }

        public int AvailableBooksCount
        {
            get
            {
                if (Books == null)
                {
                    return 0;
                }

                return Books.Count(book => book.AvailableQuantity > 0);
            }
        }

        public int ActiveRentalsCount
        {
            get
            {
                if (ActiveRentals == null)
                {
                    return 0;
                }

                return ActiveRentals.Count;
            }
        }

        public ICommand DeleteBookCommand { get; private set; }
        public ICommand RentBookCommand { get; private set; }
        public ICommand ReturnBookCommand { get; private set; }
        public ICommand SearchMembersCommand { get; private set; }
        public ICommand RefreshDataCommand { get; private set; }

        private void LoadData()
        {
            LoadBooks();
            LoadCategories();
            LoadActiveRentals();
            LoadMembers();
        }

        private bool TryGetCurrentLibraryId(out int libraryId)
        {
            libraryId = 0;

            if (_currentLibrarian == null)
            {
                return false;
            }

            if (!_currentLibrarian.Library_ID.HasValue)
            {
                return false;
            }

            libraryId = _currentLibrarian.Library_ID.Value;

            if (libraryId <= 0)
            {
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
                    BookStatusMessage = "This librarian is not assigned to a library.";
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
                    BookStatusMessage = "No books have been added to your library yet.";
                }
                else
                {
                    BookStatusMessage = string.Format("{0} books loaded successfully.", Books.Count);
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
                        .Join(
                            db.Books,
                            rental => rental.BookId,
                            book => book.Id,
                            (rental, book) => new { Rental = rental, Book = book })
                        .Where(item => !item.Rental.ReturnDate.HasValue && item.Book.LibraryId == libraryId)
                        .OrderBy(item => item.Rental.DueDate)
                        .Select(item => item.Rental)
                        .ToList();

                    ActiveRentals = new ObservableCollection<Model_Rental>(rentals);
                }

                if (ActiveRentals.Count == 0)
                {
                    RentalStatusMessage = "No active rentals at the moment.";
                }
                else
                {
                    RentalStatusMessage = string.Format("{0} active rental(s) found.", ActiveRentals.Count);
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
                {
                    var title = book.Title;
                    var author = book.Author;

                    if (title == null)
                    {
                        title = string.Empty;
                    }

                    if (author == null)
                    {
                        author = string.Empty;
                    }

                    return title.ToLower().Contains(searchLower) ||
                           author.ToLower().Contains(searchLower);
                });
            }

            FilteredBooks = new ObservableCollection<Model_Book>(filtered);
        }

        private bool CanDeleteBook()
        {
            if (SelectedBook == null)
            {
                return false;
            }

            return SelectedBook.AvailableQuantity == SelectedBook.Quantity;
        }

        private void DeleteBook()
        {
            if (SelectedBook == null)
            {
                BookStatusMessage = "Select a book to delete.";
                return;
            }

            var selectedBookTitle = SelectedBook.Title;

            var confirmation = MessageBox.Show(
                string.Format("Are you sure you want to delete \"{0}\"?", selectedBookTitle),
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
                    BookStatusMessage = "This librarian is not assigned to a library.";
                    return;
                }

                using (var db = new LibraryDbContext())
                {
                    var book = db.Books.FirstOrDefault(item =>
                        item.Id == SelectedBook.Id &&
                        item.LibraryId == libraryId &&
                        item.IsActive);

                    if (book == null)
                    {
                        BookStatusMessage = "The selected book no longer exists in your library.";
                        return;
                    }

                    var hasActiveRentals = db.Rentals.Any(rental =>
                        rental.BookId == book.Id &&
                        !rental.ReturnDate.HasValue);

                    if (hasActiveRentals)
                    {
                        BookStatusMessage = "Cannot delete a book that is currently rented out.";
                        MessageBox.Show("This book cannot be deleted because it has active rentals.", "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (book.AvailableQuantity != book.Quantity)
                    {
                        BookStatusMessage = "Cannot delete a book with inconsistent rental quantities.";
                        return;
                    }

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
                MessageBox.Show("Error deleting book: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanRentBook()
        {
            if (SelectedBook == null)
            {
                return false;
            }

            if (SelectedMember == null)
            {
                return false;
            }

            if (SelectedBook.AvailableQuantity <= 0)
            {
                return false;
            }

            if (RentalDueDate <= DateTime.Now)
            {
                return false;
            }

            return true;
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

            var confirmation = MessageBox.Show(
                string.Format("Rent \"{0}\" to {1}?", SelectedBook.Title, SelectedMember.FullName),
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
                    RentalStatusMessage = "This librarian is not assigned to a library.";
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
                                MessageBox.Show("This book has no available copies.", "Not Available", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                            book.AvailableQuantity = book.AvailableQuantity - 1;

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

                RentalStatusMessage = string.Format("Book \"{0}\" rented to {1}.", rentedBookTitle, rentedMemberName);
                MessageBox.Show(string.Format("Successfully rented \"{0}\" to {1}.", rentedBookTitle, rentedMemberName), "Rental Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Book could not be rented.";
                MessageBox.Show("Error renting book: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanReturnBook()
        {
            if (SelectedRental == null)
            {
                return false;
            }

            return true;
        }

        private void ReturnBook()
        {
            if (SelectedRental == null)
            {
                RentalStatusMessage = "Select a rental to return.";
                return;
            }

            var selectedBookTitle = SelectedRental.BookTitle;

            var confirmation = MessageBox.Show(
                string.Format("Confirm return of \"{0}\"?", selectedBookTitle),
                "Confirm Return",
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
                    RentalStatusMessage = "This librarian is not assigned to a library.";
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

                            if (book.AvailableQuantity >= book.Quantity)
                            {
                                RentalStatusMessage = "Book inventory is already full. Return cannot be completed.";
                                transaction.Rollback();
                                return;
                            }

                            rental.ReturnDate = DateTime.Now;
                            book.AvailableQuantity = book.AvailableQuantity + 1;

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

                RentalStatusMessage = string.Format("Book \"{0}\" returned.", selectedBookTitle);
                MessageBox.Show(string.Format("Successfully returned \"{0}\".", selectedBookTitle), "Return Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                SelectedRental = null;
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Book could not be returned.";
                MessageBox.Show("Error returning book: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            member.StudentId.ToLower().Contains(searchLower));
                    }

                    var members = query
                        .OrderBy(member => member.FullName)
                        .ToList();

                    Members = new ObservableCollection<Model_Member>(members);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error searching members: " + ex.Message);
            }
        }
    }
}