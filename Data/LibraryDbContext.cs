using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryManagement.Models;


namespace LibraryManagement.Data
{
    public class LibraryDbContext :DbContext
    {
        public LibraryDbContext() : base("LibraryDb") { }

        public DbSet<Model_User> Users { get; set; }

    }
}
