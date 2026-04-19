using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;



namespace LibraryManagement.Models
{
    [Table("Users")]
    public class Model_User
    {
        [Key]
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _name;
        public string Username
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _pass;
        public string Password
        {
            get { return _pass; }
            set { _pass = value; }
        }

        private string _role;
        public string Role
        {
            get { return _role; }
            set { _role = value; }
        }

        private bool _isactive;
        public bool IsActive
        {
            get { return _isactive; }
            set { _isactive = value; }
        }

    }
}
