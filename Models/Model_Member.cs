using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibraryManagement.MVVM;

namespace LibraryManagement.Models
{
    [Table("Members")]
    public class Model_Member : ViewModelBase
    {
        private int _id;
        private string _studentId;
        private string _fullName;
        private string _email;
        private string _phone;
        private string _department;
        private bool _isActive;

        [Key]
        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(50)]
        public string StudentId
        {
            get { return _studentId; }
            set { _studentId = value; OnPropertyChanged(); }
        }

        [Required]
        [StringLength(200)]
        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        public string Email
        {
            get { return _email; }
            set { _email = value; OnPropertyChanged(); }
        }

        [StringLength(20)]
        public string Phone
        {
            get { return _phone; }
            set { _phone = value; OnPropertyChanged(); }
        }

        [StringLength(100)]
        public string Department
        {
            get { return _department; }
            set { _department = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; OnPropertyChanged(); }
        }
    }
}
