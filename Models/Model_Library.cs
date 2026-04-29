using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    }
}
