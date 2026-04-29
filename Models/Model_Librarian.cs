using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Models
{
    [NotMapped]
    public class Model_Librarian : Model_User
    {
        private int _library_id;
        public int Library_ID
        {
            get { return _library_id; }
            set { _library_id = value; }
        }

        private string _library_name;
        public string LibraryName
        {
            get { return _library_name; }
            set { _library_name = value; }
        }
    }
}
