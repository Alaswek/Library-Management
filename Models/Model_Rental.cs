using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibraryManagement.MVVM;

namespace LibraryManagement.Models
{
    [Table("Rentals")]
    public class Model_Rental : ViewModelBase
    {
        private int _id;
        private int _bookId;
        private string _bookTitle;
        private int _memberId;
        private string _memberName;
        private string _studentId;
        private DateTime _rentalDate;
        private DateTime _dueDate;
        private DateTime? _returnDate;
        private string _status;

        [Key]
        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }

        [ForeignKey("Book")]
        public int BookId
        {
            get { return _bookId; }
            set { _bookId = value; OnPropertyChanged(); }
        }

        [StringLength(200)]
        public string BookTitle
        {
            get { return _bookTitle; }
            set { _bookTitle = value; OnPropertyChanged(); }
        }

        [ForeignKey("Member")]
        public int MemberId
        {
            get { return _memberId; }
            set { _memberId = value; OnPropertyChanged(); }
        }

        [StringLength(200)]
        public string MemberName
        {
            get { return _memberName; }
            set { _memberName = value; OnPropertyChanged(); }
        }

        [StringLength(50)]
        public string StudentId
        {
            get { return _studentId; }
            set { _studentId = value; OnPropertyChanged(); }
        }

        public DateTime RentalDate
        {
            get { return _rentalDate; }
            set { _rentalDate = value; OnPropertyChanged(); }
        }

        public DateTime DueDate
        {
            get { return _dueDate; }
            set { _dueDate = value; OnPropertyChanged(); }
        }

        public DateTime? ReturnDate
        {
            get { return _returnDate; }
            set
            {
                _returnDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
            }
        }

        [NotMapped]
        public string Status
        {
            get
            {
                if (ReturnDate.HasValue)
                    return "Returned";
                return DueDate < DateTime.Now ? "Overdue" : "Active";
            }
        }

        public virtual Model_Book Book { get; set; }
        public virtual Model_Member Member { get; set; }
    }
}
