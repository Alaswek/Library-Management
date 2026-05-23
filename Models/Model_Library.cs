using System;
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
        private string _openingHours;

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

        [NotMapped]
        public bool IsOpen
        {
            get { return IsOpenNow(); }
        }

        [NotMapped]
        public string Status
        {
            get
            {
                if (IsOpen)
                {
                    return "Open";
                }

                return "Closed";
            }
        }

        private bool IsOpenNow()
        {
            if (string.IsNullOrWhiteSpace(OpeningHours))
            {
                return false;
            }

            string[] parts = OpeningHours.Split('-');

            if (parts.Length != 2)
            {
                return false;
            }

            TimeSpan startTime;
            TimeSpan endTime;

            if (!TimeSpan.TryParse(parts[0], out startTime))
            {
                return false;
            }

            if (!TimeSpan.TryParse(parts[1], out endTime))
            {
                return false;
            }

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            return currentTime >= startTime && currentTime <= endTime;
        }

        public virtual ICollection<Model_Book> Books
        {
            get { return _books; }
            set { _books = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(50)]
        public string OpeningHours
        {
            get { return _openingHours; }
            set
            {
                _openingHours = value;
                OnPropertyChanged();
            }
        }

    }
}