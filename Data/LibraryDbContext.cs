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
            modelBuilder.Entity<Model_Book>().ToTable("Books");
            modelBuilder.Entity<Model_Member>().ToTable("Members");
            modelBuilder.Entity<Model_Rental>().ToTable("Rentals");

            modelBuilder.Entity<Model_Book>()
                .HasRequired(book => book.Library)
                .WithMany(library => library.Books)
                .HasForeignKey(book => book.LibraryId)
                .WillCascadeOnDelete(false);

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
        public DbSet<Model_Book> Books { get; set; }
        public DbSet<Model_Member> Members { get; set; }
        public DbSet<Model_Rental> Rentals { get; set; }

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
          END

          IF OBJECT_ID(N'[dbo].[Books]', N'U') IS NULL
          BEGIN
              CREATE TABLE [dbo].[Books]
              (
                  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  [Title] NVARCHAR(200) NOT NULL,
                  [Author] NVARCHAR(200) NOT NULL,
                  [LibraryId] INT NOT NULL,
                  [Quantity] INT NOT NULL CONSTRAINT [DF_Books_Quantity] DEFAULT 0,
                  [AvailableQuantity] INT NOT NULL CONSTRAINT [DF_Books_AvailableQuantity] DEFAULT 0,
                  [IsActive] BIT NOT NULL CONSTRAINT [DF_Books_IsActive] DEFAULT 1,
                  CONSTRAINT [FK_Books_Libraries] FOREIGN KEY ([LibraryId]) REFERENCES [dbo].[Libraries]([Id])
              )
          END

          -- ADD THIS NEW TABLE FOR MEMBERS
          IF OBJECT_ID(N'[dbo].[Members]', N'U') IS NULL
          BEGIN
              CREATE TABLE [dbo].[Members]
              (
                  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  [StudentId] NVARCHAR(50) NOT NULL,
                  [FullName] NVARCHAR(200) NOT NULL,
                  [Email] NVARCHAR(100) NULL,
                  [Phone] NVARCHAR(20) NULL,
                  [Department] NVARCHAR(100) NULL,
                  [IsActive] BIT NOT NULL CONSTRAINT [DF_Members_IsActive] DEFAULT 1
              )
          END

          -- ADD THIS NEW TABLE FOR RENTALS
          IF OBJECT_ID(N'[dbo].[Rentals]', N'U') IS NULL
          BEGIN
              CREATE TABLE [dbo].[Rentals]
              (
                  [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  [BookId] INT NOT NULL,
                  [BookTitle] NVARCHAR(200) NOT NULL,
                  [MemberId] INT NOT NULL,
                  [MemberName] NVARCHAR(200) NOT NULL,
                  [StudentId] NVARCHAR(50) NOT NULL,
                  [RentalDate] DATETIME NOT NULL,
                  [DueDate] DATETIME NOT NULL,
                  [ReturnDate] DATETIME NULL,
                  CONSTRAINT [FK_Rentals_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id]),
                  CONSTRAINT [FK_Rentals_Members] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members]([Id])
              )
          END");
        }
    }
}
