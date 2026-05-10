using LibraryManagement.Models;
using System.Data.Entity;

namespace LibraryManagement.Data
{
    public class LibraryDbContext : DbContext
    {
        static LibraryDbContext()
        {
            Database.SetInitializer<LibraryDbContext>(null);
        }

        public LibraryDbContext() : base("name=LibraryDb")
        {
            EnsureSchema();
        }

        public DbSet<Model_User> Users { get; set; }
        public DbSet<Model_Librarian> Librarians { get; set; }
        public DbSet<Model_Administrator> Admins { get; set; }
        public DbSet<Model_Library> Libraries { get; set; }
        public DbSet<Model_Book> Books { get; set; }
        public DbSet<Model_Member> Members { get; set; }
        public DbSet<Model_Rental> Rentals { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model_User>().ToTable("Users");
            modelBuilder.Entity<Model_Librarian>().ToTable("Librarians");
            modelBuilder.Entity<Model_Administrator>().ToTable("Admins");

            modelBuilder.Entity<Model_Library>().ToTable("Libraries");
            modelBuilder.Entity<Model_Book>().ToTable("Books");
            modelBuilder.Entity<Model_Member>().ToTable("Members");
            modelBuilder.Entity<Model_Rental>().ToTable("Rentals");

            modelBuilder.Entity<Model_Book>()
                .HasRequired(book => book.Library)
                .WithMany(library => library.Books)
                .HasForeignKey(book => book.LibraryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Model_User>()
                .HasOptional(user => user.Library)
                .WithMany()
                .HasForeignKey(user => user.Library_ID)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }

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

    IF COL_LENGTH(N'dbo.Libraries', N'AvailableSeats') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Libraries]
        ADD [AvailableSeats] INT NOT NULL CONSTRAINT [DF_Libraries_AvailableSeats] DEFAULT 0
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Libraries_AvailableSeats_NonNegative')
    BEGIN
        ALTER TABLE [dbo].[Libraries]
        ADD CONSTRAINT [CK_Libraries_AvailableSeats_NonNegative] CHECK ([AvailableSeats] >= 0)
    END
END

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Username] NVARCHAR(100) NOT NULL,
        [Password] NVARCHAR(255) NOT NULL,
        [Role] NVARCHAR(100) NOT NULL,
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

    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Libraries')
    BEGIN
        ALTER TABLE [dbo].[Users]
        ADD CONSTRAINT [FK_Users_Libraries] FOREIGN KEY ([Library_ID]) REFERENCES [dbo].[Libraries]([Id])
    END

    IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_Users_Username')
    BEGIN
        ALTER TABLE [dbo].[Users]
        ADD CONSTRAINT [UQ_Users_Username] UNIQUE ([Username])
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_Role_Valid')
    BEGIN
        ALTER TABLE [dbo].[Users]
        ADD CONSTRAINT [CK_Users_Role_Valid] CHECK ([Role] IN ('Administrator', 'Librarian'))
    END
END

IF OBJECT_ID(N'[dbo].[Librarians]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Librarians]
    (
        [Id] INT NOT NULL PRIMARY KEY,
        CONSTRAINT [FK_Librarians_Users]
            FOREIGN KEY ([Id]) REFERENCES [dbo].[Users]([Id])
            ON DELETE CASCADE
    )
END
ELSE
BEGIN
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Librarians_Users')
    BEGIN
        ALTER TABLE [dbo].[Librarians]
        DROP CONSTRAINT [FK_Librarians_Users]
    END

    IF COL_LENGTH(N'dbo.Librarians', N'Library_id') IS NOT NULL
    BEGIN
        DECLARE @dropLibrarianConstraints NVARCHAR(MAX) = N''

        SELECT @dropLibrarianConstraints = @dropLibrarianConstraints +
            N'ALTER TABLE [dbo].[Librarians] DROP CONSTRAINT [' + dc.name + N'];'
        FROM sys.default_constraints dc
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Librarians')
          AND COL_NAME(dc.parent_object_id, dc.parent_column_id) = N'Library_id'

        SELECT @dropLibrarianConstraints = @dropLibrarianConstraints +
            N'ALTER TABLE [dbo].[Librarians] DROP CONSTRAINT [' + fk.name + N'];'
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Librarians')
          AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = N'Library_id'

        SELECT @dropLibrarianConstraints = @dropLibrarianConstraints +
            N'ALTER TABLE [dbo].[Librarians] DROP CONSTRAINT [' + cc.name + N'];'
        FROM sys.check_constraints cc
        WHERE cc.parent_object_id = OBJECT_ID(N'dbo.Librarians')
          AND cc.definition LIKE N'%Library_id%'

        IF LEN(@dropLibrarianConstraints) > 0
        BEGIN
            EXEC sp_executesql @dropLibrarianConstraints
        END

        ALTER TABLE [dbo].[Librarians]
        DROP COLUMN [Library_id]
    END

    DELETE l
    FROM [dbo].[Librarians] l
    WHERE NOT EXISTS (
        SELECT 1
        FROM [dbo].[Users] u
        WHERE u.[Id] = l.[Id]
    )

    ALTER TABLE [dbo].[Librarians]
    ADD CONSTRAINT [FK_Librarians_Users]
        FOREIGN KEY ([Id]) REFERENCES [dbo].[Users]([Id])
        ON DELETE CASCADE
END

IF OBJECT_ID(N'[dbo].[Admins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Admins]
    (
        [Id] INT NOT NULL PRIMARY KEY,
        CONSTRAINT [FK_Admins_Users]
            FOREIGN KEY ([Id]) REFERENCES [dbo].[Users]([Id])
            ON DELETE CASCADE
    )
END
ELSE
BEGIN
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Librarians_Admis')
    BEGIN
        ALTER TABLE [dbo].[Admins]
        DROP CONSTRAINT [FK_Librarians_Admis]
    END

    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Admins_Users')
    BEGIN
        ALTER TABLE [dbo].[Admins]
        DROP CONSTRAINT [FK_Admins_Users]
    END

    DELETE a
    FROM [dbo].[Admins] a
    WHERE NOT EXISTS (
        SELECT 1
        FROM [dbo].[Users] u
        WHERE u.[Id] = a.[Id]
    )

    ALTER TABLE [dbo].[Admins]
    ADD CONSTRAINT [FK_Admins_Users]
        FOREIGN KEY ([Id]) REFERENCES [dbo].[Users]([Id])
        ON DELETE CASCADE
END

INSERT INTO [dbo].[Librarians] ([Id])
SELECT u.[Id]
FROM [dbo].[Users] u
WHERE u.[Role] = 'Librarian'
  AND NOT EXISTS (
      SELECT 1
      FROM [dbo].[Librarians] l
      WHERE l.[Id] = u.[Id]
  )

INSERT INTO [dbo].[Admins] ([Id])
SELECT u.[Id]
FROM [dbo].[Users] u
WHERE u.[Role] = 'Administrator'
  AND NOT EXISTS (
      SELECT 1
      FROM [dbo].[Admins] a
      WHERE a.[Id] = u.[Id]
  )

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
    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Books_Quantity_NonNegative')
    BEGIN
        ALTER TABLE [dbo].[Books]
        ADD CONSTRAINT [CK_Books_Quantity_NonNegative] CHECK ([Quantity] >= 0)
    END

    IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Books_AvailableQuantity_NonNegative')
    BEGIN
        ALTER TABLE [dbo].[Books]
        ADD CONSTRAINT [CK_Books_AvailableQuantity_NonNegative] CHECK ([AvailableQuantity] >= 0)
    END
END

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
    IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_Members_StudentId')
    BEGIN
        ALTER TABLE [dbo].[Members]
        ADD CONSTRAINT [UQ_Members_StudentId] UNIQUE ([StudentId])
    END
END

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