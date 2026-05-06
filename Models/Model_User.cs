using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LibraryManagement.MVVM;

namespace LibraryManagement.Models
{
    [Table("Users")]
    public class Model_User:ViewModelBase
    {
        private int _id;
        private string _name;
        private string _pass;
        private string _role;
        private bool _isactive;
        private int? _libraryId;
        private Model_Library _library;
        [Key]
        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }
        [Required]
        public string Username
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        [Required]
        public string Password
        {
            get { return _pass; }
            set { _pass = value; OnPropertyChanged(); }
        }


        //Cu asta rezolv observatia 5 dar mai trebuie ca cineva sa faca o setare manuala in BD
        //Acum nu risc asta nu cumva să fie cv problema
        [Required]
        public string Role
        {
            get { return _role; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Role cannot be empty");

                var validRoles = new[] { "Administrator", "Librarian" };
                if (!validRoles.Contains(value))
                    throw new ArgumentException($"Role must be one of: {string.Join(", ", validRoles)}");

                _role = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get { return _isactive; }
            set { _isactive = value; OnPropertyChanged(); }
        }
        //Observatie 2 pdf rezolvata
        [Column("Library_ID")]
        [ForeignKey("Library")]
        public int? Library_ID
        {
            get { return _libraryId; }
            set { _libraryId = value; OnPropertyChanged(); }
        }
        //Asta nush dc trebuie dar il las (la fel cu param declarat sus)
        public virtual Model_Library Library
        {
            get { return _library; }
            set { _library = value; OnPropertyChanged(); }
        }
    }
}

