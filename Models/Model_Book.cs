using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibraryManagement.MVVM;

namespace LibraryManagement.Models
{
    [Table("Books")]
    public class Model_Book : ViewModelBase
    {
        private int _id;
        private string _title;
        private string _author;
        private int _libraryId;
        private int _quantity;
        private int _availableQuantity;
        private bool _isActive;
        private Model_Library _library;

        [Key]
        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(200)]
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(200)]
        public string Author
        {
            get { return _author; }
            set { _author = value; OnPropertyChanged(); }
        }

        [ForeignKey("Library")]
        public int LibraryId
        {
            get { return _libraryId; }
            set { _libraryId = value; OnPropertyChanged(); }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; OnPropertyChanged(); }
        }

        public int AvailableQuantity
        {
            get { return _availableQuantity; }
            set { _availableQuantity = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; OnPropertyChanged(); }
        }

        public virtual Model_Library Library
        {
            get { return _library; }
            set { _library = value; OnPropertyChanged(); }
        }
    }
}
