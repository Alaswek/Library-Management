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
            RunSql(@"SET XACT_ABORT ON;");
            CreateTables();
            AddMissingColumns();
            CleanExistingData();
            AddConstraints();
            AddIndexes();
        }

        private void RunSql(string sql)
        {
            Database.ExecuteSqlCommand(sql);
        }

        private void CreateTables()
        {
            RunSql(@"IF OBJECT_ID(N'[dbo].[Libraries]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Libraries]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Address] NVARCHAR(300) NOT NULL,
        [IsOpen] BIT NOT NULL CONSTRAINT [DF_Libraries_IsOpen] DEFAULT ((0)),
        [OpeningHours] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Libraries_OpeningHours] DEFAULT (N''),
        CONSTRAINT [PK_Libraries] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Username] NVARCHAR(100) NOT NULL,
        [Password] NVARCHAR(255) NOT NULL,
        [Role] NVARCHAR(100) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT ((0)),
        [Library_ID] INT NULL,
        [MustChangePassword] BIT NOT NULL CONSTRAINT [DF_Users_MustChangePassword] DEFAULT ((0)),
        [PasswordResetCode] NVARCHAR(20) NULL,
        [PasswordResetCodeExpiresAt] DATETIME2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;

IF OBJECT_ID(N'[dbo].[Admins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Admins]
    (
        [Id] INT NOT NULL,
        CONSTRAINT [PK_Admins] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;

IF OBJECT_ID(N'[dbo].[Librarians]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Librarians]
    (
        [Id] INT NOT NULL,
        CONSTRAINT [PK_Librarians] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;

IF OBJECT_ID(N'[dbo].[Books]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Books]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Author] NVARCHAR(200) NOT NULL,
        [LibraryId] INT NOT NULL,
        [Quantity] INT NOT NULL CONSTRAINT [DF_Books_Quantity] DEFAULT ((0)),
        [AvailableQuantity] INT NOT NULL CONSTRAINT [DF_Books_AvailableQuantity] DEFAULT ((0)),
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Books_IsActive] DEFAULT ((1)),
        CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;

IF OBJECT_ID(N'[dbo].[Members]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Members]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [StudentId] NVARCHAR(50) NOT NULL,
        [FullName] NVARCHAR(200) NOT NULL,
        [Email] NVARCHAR(100) NULL,
        [Phone] NVARCHAR(20) NULL,
        [Department] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Members_IsActive] DEFAULT ((1)),
        CONSTRAINT [PK_Members] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;

IF OBJECT_ID(N'[dbo].[Rentals]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Rentals]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [BookId] INT NOT NULL,
        [BookTitle] NVARCHAR(200) NOT NULL,
        [MemberId] INT NOT NULL,
        [MemberName] NVARCHAR(200) NOT NULL,
        [StudentId] NVARCHAR(50) NOT NULL,
        [RentalDate] DATETIME NOT NULL,
        [DueDate] DATETIME NOT NULL,
        [ReturnDate] DATETIME NULL,
        CONSTRAINT [PK_Rentals] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;");
        }
        private void AddMissingColumns()
        {
            RunSql(@"IF COL_LENGTH(N'dbo.Libraries', N'Name') IS NULL
BEGIN
    ALTER TABLE [dbo].[Libraries]
    ADD [Name] NVARCHAR(200) NOT NULL CONSTRAINT [DF_Libraries_Name] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Libraries', N'Address') IS NULL
BEGIN
    ALTER TABLE [dbo].[Libraries]
    ADD [Address] NVARCHAR(300) NOT NULL CONSTRAINT [DF_Libraries_Address] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Libraries', N'IsOpen') IS NULL
BEGIN
    IF COL_LENGTH(N'dbo.Libraries', N'IsActive') IS NOT NULL
    BEGIN
        EXEC sp_rename N'[dbo].[Libraries].[IsActive]', N'IsOpen', N'COLUMN';
    END
    ELSE
    BEGIN
        ALTER TABLE [dbo].[Libraries]
        ADD [IsOpen] BIT NOT NULL CONSTRAINT [DF_Libraries_IsOpen] DEFAULT ((0));
    END;
END;

IF COL_LENGTH(N'dbo.Libraries', N'OpeningHours') IS NULL
BEGIN
    ALTER TABLE [dbo].[Libraries]
    ADD [OpeningHours] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Libraries_OpeningHours] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Users', N'Username') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [Username] NVARCHAR(100) NOT NULL CONSTRAINT [DF_Users_Username] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Users', N'Password') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [Password] NVARCHAR(255) NOT NULL CONSTRAINT [DF_Users_Password] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Users', N'Role') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [Role] NVARCHAR(100) NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT (N'Librarian');
END;

IF COL_LENGTH(N'dbo.Users', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [IsActive] BIT NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT ((0));
END;

IF COL_LENGTH(N'dbo.Users', N'Library_ID') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [Library_ID] INT NULL;
END;

IF COL_LENGTH(N'dbo.Users', N'MustChangePassword') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [MustChangePassword] BIT NOT NULL
        CONSTRAINT [DF_Users_MustChangePassword] DEFAULT ((0));
END;

IF COL_LENGTH(N'dbo.Users', N'PasswordResetCode') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [PasswordResetCode] NVARCHAR(20) NULL;
END;

IF COL_LENGTH(N'dbo.Users', N'PasswordResetCodeExpiresAt') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [PasswordResetCodeExpiresAt] DATETIME2 NULL;
END;

IF COL_LENGTH(N'dbo.Books', N'Title') IS NULL
BEGIN
    ALTER TABLE [dbo].[Books]
    ADD [Title] NVARCHAR(200) NOT NULL CONSTRAINT [DF_Books_Title] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Books', N'Author') IS NULL
BEGIN
    ALTER TABLE [dbo].[Books]
    ADD [Author] NVARCHAR(200) NOT NULL CONSTRAINT [DF_Books_Author] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Books', N'LibraryId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Books]
    ADD [LibraryId] INT NOT NULL CONSTRAINT [DF_Books_LibraryId] DEFAULT ((0));
END;

IF COL_LENGTH(N'dbo.Books', N'Quantity') IS NULL
BEGIN
    ALTER TABLE [dbo].[Books]
    ADD [Quantity] INT NOT NULL CONSTRAINT [DF_Books_Quantity] DEFAULT ((0));
END;

IF COL_LENGTH(N'dbo.Books', N'AvailableQuantity') IS NULL
BEGIN
    ALTER TABLE [dbo].[Books]
    ADD [AvailableQuantity] INT NOT NULL CONSTRAINT [DF_Books_AvailableQuantity] DEFAULT ((0));
END;

IF COL_LENGTH(N'dbo.Books', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Books]
    ADD [IsActive] BIT NOT NULL CONSTRAINT [DF_Books_IsActive] DEFAULT ((1));
END;

IF COL_LENGTH(N'dbo.Members', N'StudentId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Members]
    ADD [StudentId] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Members_StudentId] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Members', N'FullName') IS NULL
BEGIN
    ALTER TABLE [dbo].[Members]
    ADD [FullName] NVARCHAR(200) NOT NULL CONSTRAINT [DF_Members_FullName] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Members', N'Email') IS NULL
BEGIN
    ALTER TABLE [dbo].[Members]
    ADD [Email] NVARCHAR(100) NULL;
END;

IF COL_LENGTH(N'dbo.Members', N'Phone') IS NULL
BEGIN
    ALTER TABLE [dbo].[Members]
    ADD [Phone] NVARCHAR(20) NULL;
END;

IF COL_LENGTH(N'dbo.Members', N'Department') IS NULL
BEGIN
    ALTER TABLE [dbo].[Members]
    ADD [Department] NVARCHAR(100) NULL;
END;

IF COL_LENGTH(N'dbo.Members', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Members]
    ADD [IsActive] BIT NOT NULL CONSTRAINT [DF_Members_IsActive] DEFAULT ((1));
END;

IF COL_LENGTH(N'dbo.Rentals', N'BookId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Rentals]
    ADD [BookId] INT NOT NULL CONSTRAINT [DF_Rentals_BookId] DEFAULT ((0));
END;

IF COL_LENGTH(N'dbo.Rentals', N'BookTitle') IS NULL
BEGIN
    ALTER TABLE [dbo].[Rentals]
    ADD [BookTitle] NVARCHAR(200) NOT NULL CONSTRAINT [DF_Rentals_BookTitle] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Rentals', N'MemberId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Rentals]
    ADD [MemberId] INT NOT NULL CONSTRAINT [DF_Rentals_MemberId] DEFAULT ((0));
END;

IF COL_LENGTH(N'dbo.Rentals', N'MemberName') IS NULL
BEGIN
    ALTER TABLE [dbo].[Rentals]
    ADD [MemberName] NVARCHAR(200) NOT NULL CONSTRAINT [DF_Rentals_MemberName] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Rentals', N'StudentId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Rentals]
    ADD [StudentId] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Rentals_StudentId] DEFAULT (N'');
END;

IF COL_LENGTH(N'dbo.Rentals', N'RentalDate') IS NULL
BEGIN
    ALTER TABLE [dbo].[Rentals]
    ADD [RentalDate] DATETIME NOT NULL CONSTRAINT [DF_Rentals_RentalDate] DEFAULT (GETDATE());
END;

IF COL_LENGTH(N'dbo.Rentals', N'DueDate') IS NULL
BEGIN
    ALTER TABLE [dbo].[Rentals]
    ADD [DueDate] DATETIME NOT NULL CONSTRAINT [DF_Rentals_DueDate] DEFAULT (GETDATE());
END;

IF COL_LENGTH(N'dbo.Rentals', N'ReturnDate') IS NULL
BEGIN
    ALTER TABLE [dbo].[Rentals]
    ADD [ReturnDate] DATETIME NULL;
END;");
        }
        private void CleanExistingData()
        {
            RunSql(@"UPDATE [dbo].[Users]
SET [Username] = CONCAT(N'user_', [Id])
WHERE [Username] IS NULL OR LTRIM(RTRIM([Username])) = N'';

UPDATE [dbo].[Users]
SET [Password] = N''
WHERE [Password] IS NULL;

UPDATE [dbo].[Users]
SET [Role] = N'Librarian'
WHERE [Role] IS NULL
   OR [Role] NOT IN (N'Administrator', N'Librarian');

;WITH DuplicateUsers AS
(
    SELECT
        [Id],
        [Username],
        ROW_NUMBER() OVER (PARTITION BY [Username] ORDER BY [Id]) AS RowNumber
    FROM [dbo].[Users]
)
UPDATE u
SET [Username] = LEFT(CONCAT(u.[Username], N'_', CONVERT(NVARCHAR(20), u.[Id])), 100)
FROM [dbo].[Users] u
INNER JOIN DuplicateUsers d ON d.[Id] = u.[Id]
WHERE d.RowNumber > 1;

UPDATE u
SET [Library_ID] = NULL
FROM [dbo].[Users] u
WHERE u.[Library_ID] IS NOT NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[Libraries] l
      WHERE l.[Id] = u.[Library_ID]
  );

INSERT INTO [dbo].[Libraries] ([Name], [Address], [IsOpen], [OpeningHours])
SELECT N'Default Library', N'', 0, N''
WHERE EXISTS
(
    SELECT 1
    FROM [dbo].[Books] b
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[Libraries] l
        WHERE l.[Id] = b.[LibraryId]
    )
)
AND NOT EXISTS (SELECT 1 FROM [dbo].[Libraries]);

DECLARE @FallbackLibraryId INT;
SELECT @FallbackLibraryId = MIN([Id]) FROM [dbo].[Libraries];

UPDATE b
SET [LibraryId] = @FallbackLibraryId
FROM [dbo].[Books] b
WHERE @FallbackLibraryId IS NOT NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[Libraries] l
      WHERE l.[Id] = b.[LibraryId]
  );

UPDATE [dbo].[Books]
SET [Quantity] = 0
WHERE [Quantity] < 0;

UPDATE [dbo].[Books]
SET [AvailableQuantity] = 0
WHERE [AvailableQuantity] < 0;

UPDATE [dbo].[Books]
SET [AvailableQuantity] = [Quantity]
WHERE [AvailableQuantity] > [Quantity];

UPDATE [dbo].[Members]
SET [StudentId] = CONCAT(N'student_', [Id])
WHERE [StudentId] IS NULL OR LTRIM(RTRIM([StudentId])) = N'';

UPDATE [dbo].[Members]
SET [FullName] = N''
WHERE [FullName] IS NULL;

;WITH DuplicateMembers AS
(
    SELECT
        [Id],
        [StudentId],
        ROW_NUMBER() OVER (PARTITION BY [StudentId] ORDER BY [Id]) AS RowNumber
    FROM [dbo].[Members]
)
UPDATE m
SET [StudentId] = LEFT(CONCAT(m.[StudentId], N'_', CONVERT(NVARCHAR(20), m.[Id])), 50)
FROM [dbo].[Members] m
INNER JOIN DuplicateMembers d ON d.[Id] = m.[Id]
WHERE d.RowNumber > 1;

DELETE r
FROM [dbo].[Rentals] r
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[Books] b
    WHERE b.[Id] = r.[BookId]
)
OR NOT EXISTS
(
    SELECT 1
    FROM [dbo].[Members] m
    WHERE m.[Id] = r.[MemberId]
);

UPDATE r
SET
    [BookTitle] = b.[Title],
    [MemberName] = m.[FullName],
    [StudentId] = m.[StudentId]
FROM [dbo].[Rentals] r
INNER JOIN [dbo].[Books] b ON b.[Id] = r.[BookId]
INNER JOIN [dbo].[Members] m ON m.[Id] = r.[MemberId]
WHERE r.[BookTitle] IS NULL
   OR LTRIM(RTRIM(r.[BookTitle])) = N''
   OR r.[MemberName] IS NULL
   OR LTRIM(RTRIM(r.[MemberName])) = N''
   OR r.[StudentId] IS NULL
   OR LTRIM(RTRIM(r.[StudentId])) = N'';

UPDATE [dbo].[Rentals]
SET [RentalDate] = GETDATE()
WHERE [RentalDate] IS NULL;

UPDATE [dbo].[Rentals]
SET [DueDate] = DATEADD(DAY, 14, [RentalDate])
WHERE [DueDate] IS NULL;

DELETE a
FROM [dbo].[Admins] a
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[Users] u
    WHERE u.[Id] = a.[Id]
      AND u.[Role] = N'Administrator'
);

DELETE l
FROM [dbo].[Librarians] l
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[Users] u
    WHERE u.[Id] = l.[Id]
      AND u.[Role] = N'Librarian'
);

INSERT INTO [dbo].[Admins] ([Id])
SELECT u.[Id]
FROM [dbo].[Users] u
WHERE u.[Role] = N'Administrator'
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[Admins] a
      WHERE a.[Id] = u.[Id]
  );

INSERT INTO [dbo].[Librarians] ([Id])
SELECT u.[Id]
FROM [dbo].[Users] u
WHERE u.[Role] = N'Librarian'
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[Librarians] l
      WHERE l.[Id] = u.[Id]
  );");
        }
        private void AddConstraints()
        {
            RunSql(@"IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Users_Libraries'
      AND parent_object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    ALTER TABLE [dbo].[Users] WITH CHECK
    ADD CONSTRAINT [FK_Users_Libraries]
    FOREIGN KEY ([Library_ID]) REFERENCES [dbo].[Libraries]([Id]);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE name = N'UQ_Users_Username'
      AND parent_object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD CONSTRAINT [UQ_Users_Username] UNIQUE NONCLUSTERED ([Username] ASC);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Users_Role_Valid'
      AND parent_object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    ALTER TABLE [dbo].[Users] WITH CHECK
    ADD CONSTRAINT [CK_Users_Role_Valid]
    CHECK ([Role] = N'Librarian' OR [Role] = N'Administrator');
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Admins_Users'
      AND parent_object_id = OBJECT_ID(N'dbo.Admins')
)
BEGIN
    ALTER TABLE [dbo].[Admins] WITH CHECK
    ADD CONSTRAINT [FK_Admins_Users]
    FOREIGN KEY ([Id]) REFERENCES [dbo].[Users]([Id])
    ON DELETE CASCADE;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Librarians_Users'
      AND parent_object_id = OBJECT_ID(N'dbo.Librarians')
)
BEGIN
    ALTER TABLE [dbo].[Librarians] WITH CHECK
    ADD CONSTRAINT [FK_Librarians_Users]
    FOREIGN KEY ([Id]) REFERENCES [dbo].[Users]([Id])
    ON DELETE CASCADE;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Books_Libraries'
      AND parent_object_id = OBJECT_ID(N'dbo.Books')
)
BEGIN
    ALTER TABLE [dbo].[Books] WITH CHECK
    ADD CONSTRAINT [FK_Books_Libraries]
    FOREIGN KEY ([LibraryId]) REFERENCES [dbo].[Libraries]([Id]);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Books_Quantity_NonNegative'
      AND parent_object_id = OBJECT_ID(N'dbo.Books')
)
BEGIN
    ALTER TABLE [dbo].[Books] WITH CHECK
    ADD CONSTRAINT [CK_Books_Quantity_NonNegative]
    CHECK ([Quantity] >= 0);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Books_AvailableQuantity_NonNegative'
      AND parent_object_id = OBJECT_ID(N'dbo.Books')
)
BEGIN
    ALTER TABLE [dbo].[Books] WITH CHECK
    ADD CONSTRAINT [CK_Books_AvailableQuantity_NonNegative]
    CHECK ([AvailableQuantity] >= 0);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Books_AvailableQuantity_LessOrEqual_Quantity'
      AND parent_object_id = OBJECT_ID(N'dbo.Books')
)
BEGIN
    ALTER TABLE [dbo].[Books] WITH CHECK
    ADD CONSTRAINT [CK_Books_AvailableQuantity_LessOrEqual_Quantity]
    CHECK ([AvailableQuantity] <= [Quantity]);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE name = N'UQ_Members_StudentId'
      AND parent_object_id = OBJECT_ID(N'dbo.Members')
)
BEGIN
    ALTER TABLE [dbo].[Members]
    ADD CONSTRAINT [UQ_Members_StudentId] UNIQUE NONCLUSTERED ([StudentId] ASC);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Rentals_Books'
      AND parent_object_id = OBJECT_ID(N'dbo.Rentals')
)
BEGIN
    ALTER TABLE [dbo].[Rentals] WITH CHECK
    ADD CONSTRAINT [FK_Rentals_Books]
    FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books]([Id]);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Rentals_Members'
      AND parent_object_id = OBJECT_ID(N'dbo.Rentals')
)
BEGIN
    ALTER TABLE [dbo].[Rentals] WITH CHECK
    ADD CONSTRAINT [FK_Rentals_Members]
    FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members]([Id]);
END;");
        }
        private void AddIndexes()
        {
            RunSql(@"IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Users_Library_ID'
      AND object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Users_Library_ID]
    ON [dbo].[Users] ([Library_ID] ASC);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Books_LibraryId'
      AND object_id = OBJECT_ID(N'dbo.Books')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Books_LibraryId]
    ON [dbo].[Books] ([LibraryId] ASC);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Rentals_BookId'
      AND object_id = OBJECT_ID(N'dbo.Rentals')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Rentals_BookId]
    ON [dbo].[Rentals] ([BookId] ASC);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Rentals_MemberId'
      AND object_id = OBJECT_ID(N'dbo.Rentals')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Rentals_MemberId]
    ON [dbo].[Rentals] ([MemberId] ASC);
END;");
        }
    }
}
