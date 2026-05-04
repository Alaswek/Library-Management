using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;

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
        private string _selectedCategory;
        private Model_Book _selectedBook;
        private Model_Member _selectedMember;
        private Model_Rental _selectedRental;
        private DateTime _rentalDueDate;
        private string _bookStatusMessage;
        private string _rentalStatusMessage;

        public Librarian_BookViewModel(Model_User currentLibrarian)
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
            get { return string.Format("Welcome, {0}", _currentLibrarian != null ? _currentLibrarian.Username : "Librarian"); }
        }

        public ObservableCollection<Model_Book> Books
        {
            get { return _books; }
            set { SetProperty(ref _books, value); }
        }

        public ObservableCollection<Model_Book> FilteredBooks
        {
            get { return _filteredBooks; }
            set { SetProperty(ref _filteredBooks, value); }
        }

        public ObservableCollection<Model_Rental> ActiveRentals
        {
            get { return _activeRentals; }
            set { SetProperty(ref _activeRentals, value); }
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
            set { SetProperty(ref _rentalDueDate, value); }
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
            get { return Books.Count; }
        }

        public int AvailableBooksCount
        {
            get { return Books.Count(b => b.AvailableQuantity > 0); }
        }

        public int ActiveRentalsCount
        {
            get { return ActiveRentals.Count; }
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

        private void LoadBooks()
        {
            try
            {
                using (var db = new LibraryDbContext())
                {
                    var books = db.Books
                        .Include(b => b.Library)
                        .Where(b => b.IsActive)
                        .OrderBy(b => b.Title)
                        .ToList();
                    Books = new ObservableCollection<Model_Book>(books);
                    FilterBooks();
                    BookStatusMessage = Books.Count == 0 ? "No books have been added to the library yet." : string.Format("{0} books loaded successfully.", Books.Count);
                    OnPropertyChanged(nameof(TotalBooks));
                    OnPropertyChanged(nameof(AvailableBooksCount));
                }
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
                using (var db = new LibraryDbContext())
                {
                    var libraryNames = db.Libraries
                        .Where(l => l.IsOpen)
                        .Select(l => l.Name)
                        .Distinct()
                        .OrderBy(n => n)
                        .ToList();
                    foreach (var name in libraryNames)
                    {
                        Categories.Add(name);
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
                using (var db = new LibraryDbContext())
                {
                    var rentals = db.Rentals
                        .Where(r => !r.ReturnDate.HasValue)
                        .OrderBy(r => r.DueDate)
                        .ToList();
                    ActiveRentals = new ObservableCollection<Model_Rental>(rentals);
                    RentalStatusMessage = ActiveRentals.Count == 0 ? "No active rentals at the moment." : string.Format("{0} active rental(s) found.", ActiveRentals.Count);
                    OnPropertyChanged(nameof(ActiveRentalsCount));
                }
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
                        .Where(m => m.IsActive)
                        .OrderBy(m => m.FullName)
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
            var filtered = Books.AsEnumerable();
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All Categories")
            {
                filtered = filtered.Where(b => b.Library != null && b.Library.Name == SelectedCategory);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(b => (b.Title ?? string.Empty).ToLower().Contains(searchLower) || (b.Author ?? string.Empty).ToLower().Contains(searchLower));
            }
            FilteredBooks = new ObservableCollection<Model_Book>(filtered);
        }

        private bool CanDeleteBook()
        {
            return SelectedBook != null && SelectedBook.AvailableQuantity == SelectedBook.Quantity;
        }

        private void DeleteBook()
        {
            if (SelectedBook == null)
            {
                BookStatusMessage = "Select a book to delete.";
                return;
            }
            if (SelectedBook.AvailableQuantity != SelectedBook.Quantity)
            {
                BookStatusMessage = "Cannot delete a book that is currently rented out.";
                MessageBox.Show("This book cannot be deleted because it has active rentals.", "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var confirmation = MessageBox.Show(string.Format("Are you sure you want to delete \"{0}\"?", SelectedBook.Title), "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                using (var db = new LibraryDbContext())
                {
                    var book = db.Books.FirstOrDefault(b => b.Id == SelectedBook.Id);
                    if (book == null)
                    {
                        BookStatusMessage = "The selected book no longer exists.";
                        return;
                    }
                    book.IsActive = false;
                    db.SaveChanges();
                }
                LoadBooks();
                BookStatusMessage = string.Format("Book \"{0}\" has been deleted.", SelectedBook.Title);
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
            return SelectedBook != null && SelectedMember != null && SelectedBook.AvailableQuantity > 0;
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
            if (SelectedBook.AvailableQuantity <= 0)
            {
                RentalStatusMessage = "This book is not available for rent.";
                MessageBox.Show("This book has no available copies.", "Not Available", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var confirmation = MessageBox.Show(string.Format("Rent \"{0}\" to {1}?", SelectedBook.Title, SelectedMember.FullName), "Confirm Rental", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                using (var db = new LibraryDbContext())
                {
                    var rental = new Model_Rental
                    {
                        BookId = SelectedBook.Id,
                        BookTitle = SelectedBook.Title,
                        MemberId = SelectedMember.Id,
                        MemberName = SelectedMember.FullName,
                        StudentId = SelectedMember.StudentId,
                        RentalDate = DateTime.Now,
                        DueDate = RentalDueDate
                    };
                    db.Rentals.Add(rental);
                    var book = db.Books.FirstOrDefault(b => b.Id == SelectedBook.Id);
                    if (book != null)
                    {
                        book.AvailableQuantity--;
                    }
                    db.SaveChanges();
                }
                LoadBooks();
                LoadActiveRentals();
                RentalStatusMessage = string.Format("Book \"{0}\" rented to {1}.", SelectedBook.Title, SelectedMember.FullName);
                MessageBox.Show(string.Format("Successfully rented \"{0}\" to {1}.", SelectedBook.Title, SelectedMember.FullName), "Rental Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Book could not be rented.";
                MessageBox.Show("Error renting book: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanReturnBook()
        {
            return SelectedRental != null;
        }

        private void ReturnBook()
        {
            if (SelectedRental == null)
            {
                RentalStatusMessage = "Select a rental to return.";
                return;
            }
            var confirmation = MessageBox.Show(string.Format("Confirm return of \"{0}\"?", SelectedRental.BookTitle), "Confirm Return", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                using (var db = new LibraryDbContext())
                {
                    var rental = db.Rentals.FirstOrDefault(r => r.Id == SelectedRental.Id);
                    if (rental != null)
                    {
                        rental.ReturnDate = DateTime.Now;
                    }
                    var book = db.Books.FirstOrDefault(b => b.Id == SelectedRental.BookId);
                    if (book != null)
                    {
                        book.AvailableQuantity++;
                    }
                    db.SaveChanges();
                }
                LoadBooks();
                LoadActiveRentals();
                RentalStatusMessage = string.Format("Book \"{0}\" returned.", SelectedRental.BookTitle);
                MessageBox.Show(string.Format("Successfully returned \"{0}\".", SelectedRental.BookTitle), "Return Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                SelectedRental = null;
            }
            catch (Exception ex)
            {
                RentalStatusMessage = "Book could not be returned.";
                MessageBox.Show("Error returning book: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchMembers()
        {
            MessageBox.Show("Search for members by typing in the dropdown.", "Search Members", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}