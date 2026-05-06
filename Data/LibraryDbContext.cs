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

        public DbSet<Model_User> Users { get; set; }
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
        [IsOpen] BIT NOT NULL CONSTRAINT [DF_Libraries_IsOpen] DEFAULT 1,
        [AvailableSeats] INT NOT NULL CONSTRAINT [DF_Libraries_AvailableSeats] DEFAULT 0,
        CONSTRAINT [CK_Libraries_AvailableSeats_NonNegative] CHECK ([AvailableSeats] >= 0)
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

    -- Am pus AvailableSeats
    IF COL_LENGTH(N'dbo.Libraries', N'AvailableSeats') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Libraries]
        ADD [AvailableSeats] INT NOT NULL CONSTRAINT [DF_Libraries_AvailableSeats] DEFAULT 0
    END

    -- I-am dat si constrangere
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Libraries_AvailableSeats_NonNegative')
    BEGIN
        ALTER TABLE [dbo].[Libraries]
        ADD CONSTRAINT [CK_Libraries_AvailableSeats_NonNegative] CHECK ([AvailableSeats] >= 0)
    END
END
                 
/*Aici rezolv observatia 1 din acest fisier*/
       -- CREATE USERS TABLE IF NOT EXISTS
IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Username] NVARCHAR(100) NOT NULL,
        [Password] NVARCHAR(100) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT 1,
        [Library_ID] INT NULL,
        CONSTRAINT [FK_Users_Libraries] FOREIGN KEY ([Library_ID]) REFERENCES [dbo].[Libraries]([Id]),
        CONSTRAINT [UQ_Users_Username] UNIQUE ([Username]),
        CONSTRAINT [CK_Users_Role_Valid] CHECK ([Role] IN ('Administrator', 'Librarian'))
    )
END
ELSE
BEGIN
    IF COL_LENGTH(N'dbo.Users', N'Library_ID') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Users]
        ADD [Library_ID] INT NULL
    END
    
    -- Am pus FK boss
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Libraries')
    BEGIN
        ALTER TABLE [dbo].[Users]
        ADD CONSTRAINT [FK_Users_Libraries] FOREIGN KEY ([Library_ID]) REFERENCES [dbo].[Libraries]([Id])
    END

    -- Am pus unique
    IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_Users_Username')
    BEGIN
        ALTER TABLE [dbo].[Users]
        ADD CONSTRAINT [UQ_Users_Username] UNIQUE ([Username])
    END

    -- Am pus verificare de rol predefinit
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_Role_Valid')
    BEGIN
        ALTER TABLE [dbo].[Users]
        ADD CONSTRAINT [CK_Users_Role_Valid] CHECK ([Role] IN ('Administrator', 'Librarian'))
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
        CONSTRAINT [FK_Books_Libraries] FOREIGN KEY ([LibraryId]) REFERENCES [dbo].[Libraries]([Id]),
        CONSTRAINT [CK_Books_Quantity_NonNegative] CHECK ([Quantity] >= 0),
        CONSTRAINT [CK_Books_AvailableQuantity_NonNegative] CHECK ([AvailableQuantity] >= 0)
    )
END
ELSE
BEGIN
    --Observatia 3 rezolvata aici ca sa nu mai pot seta valori negative
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Books_Quantity_NonNegative')
    BEGIN
        ALTER TABLE [dbo].[Books] ADD CONSTRAINT [CK_Books_Quantity_NonNegative] CHECK ([Quantity] >= 0)
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Books_AvailableQuantity_NonNegative')
    BEGIN
        ALTER TABLE [dbo].[Books] ADD CONSTRAINT [CK_Books_AvailableQuantity_NonNegative] CHECK ([AvailableQuantity] >= 0)
    END
END

          -- CREATE MEMBERS TABLE IF NOT EXISTS
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
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Members_IsActive] DEFAULT 1,
        CONSTRAINT [UQ_Members_StudentId] UNIQUE ([StudentId])
    )
END
ELSE
BEGIN
    -- Am adaugat si aicia unique pentru Members.StudentId
    IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_Members_StudentId')
    BEGIN
        ALTER TABLE [dbo].[Members]
        ADD CONSTRAINT [UQ_Members_StudentId] UNIQUE ([StudentId])
    END
END

          -- CREATE RENTALS TABLE IF NOT EXISTS
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