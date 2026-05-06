using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using LibraryManagement.MVVM;

namespace LibraryManagement.Models
{
    [Table("Libraries")]
    public class Model_Library : ViewModelBase
    {
        private int _id;
        private string _name;
        private string _address;
        private bool _isOpen;
        private ICollection<Model_Book> _books;

        public Model_Library()
        {
            _books = new List<Model_Book>();
        }

        [Key]
        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(200)]
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(300)]
        public string Address
        {
            get { return _address; }
            set { _address = value; OnPropertyChanged(); }
        }

        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen = value; OnPropertyChanged(); }
        }

        public virtual ICollection<Model_Book> Books
        {
            get { return _books; }
            set { _books = value; OnPropertyChanged(); }
        }
        //L-am pus si aici pe asta ca e si in DB si acuma l-am vz
        private int _availableSeats;

        [Range(0, int.MaxValue, ErrorMessage = "Available seats cannot be negative")]
        public int AvailableSeats
        {
            get { return _availableSeats; }
            set
            {
                if (value < 0) throw new ArgumentException("Available seats cannot be negative");
                _availableSeats = value;
                OnPropertyChanged();
            }
        }
    }
}
