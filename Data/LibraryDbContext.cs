using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LibraryManagement.Models;


namespace LibraryManagement.Data
{
    public class LibraryDbContext : DbContext
    {
        // Disable database initialization
        // This prevents Entity Framework from trying to create or modify the database schema
        // since we are using an existing database with a specific structure.
        static LibraryDbContext()
        {
            Database.SetInitializer<LibraryDbContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model_User>().ToTable("Users");

            modelBuilder.Ignore<Model_Librarian>();
            modelBuilder.Ignore<Model_Administrator>();


            base.OnModelCreating(modelBuilder);
        }



        public LibraryDbContext() : base("name=LibraryDb") 
        { 
        }


        public DbSet<Model_User> Users { get; set;  }
    }
}
