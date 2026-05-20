using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibraryManagement.MVVM;

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
        private bool _mustChangePassword;
        private string _passwordResetCode;
        private DateTime? _passwordResetCodeExpiresAt;

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
        [StringLength(100)]
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
        [StringLength(255)]
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
        [StringLength(100)]
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

        public bool MustChangePassword
        {
            get { return _mustChangePassword; }
            set
            {
                _mustChangePassword = value;
                OnPropertyChanged();
            }
        }

        [StringLength(20)]
        public string PasswordResetCode
        {
            get { return _passwordResetCode; }
            set
            {
                _passwordResetCode = value;
                OnPropertyChanged();
            }
        }

        public DateTime? PasswordResetCodeExpiresAt
        {
            get { return _passwordResetCodeExpiresAt; }
            set
            {
                _passwordResetCodeExpiresAt = value;
                OnPropertyChanged();
            }
        }
    }
}