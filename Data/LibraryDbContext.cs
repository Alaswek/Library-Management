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
            modelBuilder.Entity<Model_Library>().ToTable("Libraries");

            modelBuilder.Ignore<Model_Librarian>();
            modelBuilder.Ignore<Model_Administrator>();


            base.OnModelCreating(modelBuilder);
        }



        public LibraryDbContext() : base("name=LibraryDb") 
        {
            EnsureSchema();
        }


        public DbSet<Model_User> Users { get; set;  }
        public DbSet<Model_Library> Libraries { get; set; }

        private void EnsureSchema()
        {
            Database.ExecuteSqlCommand(
                @"IF OBJECT_ID(N'[dbo].[Libraries]', N'U') IS NULL
                  BEGIN
                      CREATE TABLE [dbo].[Libraries]
                      (
                          [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                          [Name] NVARCHAR(200) NOT NULL,
                          [Address] NVARCHAR(300) NOT NULL,
                          [IsOpen] BIT NOT NULL CONSTRAINT [DF_Libraries_IsOpen] DEFAULT 1
                      )
                  END
                  ELSE
                  BEGIN
                      IF COL_LENGTH(N'dbo.Libraries', N'IsOpen') IS NULL
                      BEGIN
                          IF COL_LENGTH(N'dbo.Libraries', N'IsActive') IS NOT NULL
                          BEGIN
                              EXEC sp_rename N'[dbo].[Libraries].[IsActive]', N'IsOpen', N'COLUMN'
                          END
                          ELSE
                          BEGIN
                              ALTER TABLE [dbo].[Libraries]
                              ADD [IsOpen] BIT NOT NULL CONSTRAINT [DF_Libraries_IsOpen] DEFAULT 1
                          END
                      END
                  END

                  IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
                  BEGIN
                      IF COL_LENGTH(N'dbo.Users', N'Library_ID') IS NULL
                      BEGIN
                          ALTER TABLE [dbo].[Users]
                          ADD [Library_ID] INT NULL
                      END
                  END");
        }
    }
}
