using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("Librarians")]
    public class Model_Librarian : Model_User
    {
        private string _libraryName;

        [NotMapped]
        public string LibraryName
        {
            get
            {
                return _libraryName;
            }
            set
            {
                _libraryName = value;
                OnPropertyChanged();
            }
        }
    }
}