using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibraryManagement.MVVM;
using System.Linq;

namespace LibraryManagement.Models
{
    [Table("Users")]
    public class Model_User : ViewModelBase
    {
        private int _id;
        private string _username;
        private string _password;
        private string _role;
        private bool _isActive;
        private int? _libraryId;
        private Model_Library _library;

        [Key]
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        [Required]
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        [Required]
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        [Required]
        public string Role
        {
            get { return _role; }
            set
            {
                _role = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        [Column("Library_ID")]
        public int? Library_ID
        {
            get { return _libraryId; }
            set
            {
                _libraryId = value;
                OnPropertyChanged();
            }
        }

        public virtual Model_Library Library
        {
            get { return _library; }
            set
            {
                _library = value;
                OnPropertyChanged();
            }
        }
    }
}