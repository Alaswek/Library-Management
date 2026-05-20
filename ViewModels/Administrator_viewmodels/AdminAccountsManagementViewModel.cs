using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace LibraryManagement.ViewModels.Administrator_viewmodels
{
    public class AdminAccountsManagementViewModel : ViewModelBase
    {
        private readonly Model_User _currentAdmin;

        private ObservableCollection<Model_User> _accounts;
        private ObservableCollection<Model_Library> _libraries;

        private Model_User _selectedAccount;
        private int _editingAccountId;

        private bool _isInitialized;
        private string _accountUsername;
        private string _accountPassword;
        private string _selectedRole;
        private string _selectedFilterRole;
        private int _selectedAccountLibraryId;
        private bool _isAccountActive;
        private string _accountStatusMessage;

        private const int UsernameMaxLength = 100;
        private const int PasswordMaxLength = 255;
        private const int PasswordResetCodeLength = 6;
        private const int PasswordResetCodeExpirationMinutes = 15;

        public AdminAccountsManagementViewModel()
            : this(null)
        {
        }

        public AdminAccountsManagementViewModel(Model_User currentAdmin)
        {
            _currentAdmin = currentAdmin;
            _accounts = new ObservableCollection<Model_User>();
            _libraries = new ObservableCollection<Model_Library>();

            AvailableRoles = new ObservableCollection<string>
            {
                "Administrator",
                "Librarian"
            };

            FilterRoles = new ObservableCollection<string>
            {
                "All",
                "Administrator",
                "Librarian"
            };

            SaveAccountCommand = new RelayCommand(_ => SaveAccount());

            EditSelectedAccountCommand = new RelayCommand(
                parameter => StartEditingSelectedAccount(parameter as Model_User));

            DeleteSelectedAccountCommand = new RelayCommand(
                parameter => DeleteSelectedAccount(parameter as Model_User));

            ResetSelectedAccountPasswordCommand = new RelayCommand(
                parameter => ResetSelectedAccountPassword(parameter as Model_User));

            ClearAccountFormCommand = new RelayCommand(_ => ResetAccountForm());
            RefreshAccountsCommand = new RelayCommand(_ => LoadAccounts());

            _selectedFilterRole = "All";

            ResetAccountForm();
            AccountStatusMessage = "Ready. Initialize the account manager to load data.";
        }

        public string WelcomeMessage
        {
            get
            {
                if (_currentAdmin != null && !string.IsNullOrWhiteSpace(_currentAdmin.Username))
                {
                    return string.Format("Welcome, {0}", _currentAdmin.Username);
                }

                return "Welcome, Administrator";
            }
        }

        public ObservableCollection<Model_User> Accounts
        {
            get { return _accounts; }
            set
            {
                if (SetProperty(ref _accounts, value))
                {
                    OnPropertyChanged(nameof(TotalAccounts));
                    OnPropertyChanged(nameof(ActiveAccounts));
                    OnPropertyChanged(nameof(AdministratorAccounts));
                    OnPropertyChanged(nameof(LibrarianAccounts));
                }
            }
        }

        public ObservableCollection<Model_Library> Libraries
        {
            get { return _libraries; }
            set { SetProperty(ref _libraries, value); }
        }

        public ObservableCollection<string> AvailableRoles { get; private set; }
        public ObservableCollection<string> FilterRoles { get; private set; }

        public Model_User SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                if (SetProperty(ref _selectedAccount, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string AccountUsername
        {
            get { return _accountUsername; }
            set { SetProperty(ref _accountUsername, value); }
        }

        public string AccountPassword
        {
            get { return _accountPassword; }
            set { SetProperty(ref _accountPassword, value); }
        }

        public string SelectedRole
        {
            get { return _selectedRole; }
            set
            {
                if (SetProperty(ref _selectedRole, NormalizeRole(value)))
                {
                    if (!IsLibrarySelectionRequired)
                    {
                        SelectedAccountLibraryId = 0;
                    }

                    OnPropertyChanged(nameof(IsLibrarySelectionRequired));
                    OnPropertyChanged(nameof(AccountFormTitle));
                }
            }
        }

        public string SelectedFilterRole
        {
            get { return _selectedFilterRole; }
            set
            {
                if (SetProperty(ref _selectedFilterRole, NormalizeFilterRole(value)))
                {
                    LoadAccounts();
                }
            }
        }

        public int SelectedAccountLibraryId
        {
            get { return _selectedAccountLibraryId; }
            set { SetProperty(ref _selectedAccountLibraryId, value); }
        }

        public bool IsAccountActive
        {
            get { return _isAccountActive; }
            set { SetProperty(ref _isAccountActive, value); }
        }

        public string AccountStatusMessage
        {
            get { return _accountStatusMessage; }
            set { SetProperty(ref _accountStatusMessage, value); }
        }

        public bool IsAccountEditMode
        {
            get { return _editingAccountId > 0; }
        }

        public bool IsPasswordEnabled
        {
            get { return !IsAccountEditMode; }
        }

        public string AccountFormTitle
        {
            get
            {
                if (IsAccountEditMode)
                {
                    return "Edit Account";
                }

                return "Create Account";
            }
        }

        public bool IsLibrarySelectionRequired
        {
            get { return IsLibrarianRole(SelectedRole); }
        }

        public int TotalAccounts
        {
            get
            {
                if (Accounts == null)
                {
                    return 0;
                }

                return Accounts.Count;
            }
        }

        public int ActiveAccounts
        {
            get
            {
                if (Accounts == null)
                {
                    return 0;
                }

                return Accounts.Count(account => account != null && account.IsActive);
            }
        }

        public int AdministratorAccounts
        {
            get
            {
                if (Accounts == null)
                {
                    return 0;
                }

                return Accounts.Count(account => account != null && IsAdministratorRole(account.Role));
            }
        }

        public int LibrarianAccounts
        {
            get
            {
                if (Accounts == null)
                {
                    return 0;
                }

                return Accounts.Count(account => account != null && IsLibrarianRole(account.Role));
            }
        }

        public ICommand SaveAccountCommand { get; private set; }
        public ICommand EditSelectedAccountCommand { get; private set; }
        public ICommand ResetSelectedAccountPasswordCommand { get; private set; }
        public ICommand DeleteSelectedAccountCommand { get; private set; }
        public ICommand ClearAccountFormCommand { get; private set; }
        public ICommand RefreshAccountsCommand { get; private set; }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            LoadLibraries();
            LoadAccounts();
        }

        private void LoadLibraries()
        {
            try
            {
                using (var db = new LibraryDbContext())
                {
                    Libraries = new ObservableCollection<Model_Library>(
                        db.Libraries
                          .OrderBy(library => library.Name)
                          .ToList());
                }
            }
            catch (Exception ex)
            {
                AccountStatusMessage = "Libraries could not be loaded.";
                MessageBox.Show("Error loading libraries: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAccounts()
        {
            try
            {
                using (var db = new LibraryDbContext())
                {
                    var accounts = new List<Model_User>();
                    var filterRole = NormalizeFilterRole(SelectedFilterRole);

                    if (filterRole == "All" || filterRole == "Administrator")
                    {
                        accounts.AddRange(
                            db.Admins
                              .Include(admin => admin.Library)
                              .ToList());
                    }

                    if (filterRole == "All" || filterRole == "Librarian")
                    {
                        accounts.AddRange(
                            db.Librarians
                              .Include(librarian => librarian.Library)
                              .ToList());
                    }

                    Accounts = new ObservableCollection<Model_User>(
                        accounts
                            .Where(account => account != null)
                            .OrderBy(account => account.Role ?? string.Empty)
                            .ThenBy(account => account.Username ?? string.Empty)
                            .ToList());
                }

                if (Accounts.Count == 0)
                {
                    AccountStatusMessage = "No accounts have been created yet.";
                }
                else
                {
                    AccountStatusMessage = "Accounts loaded successfully.";
                }

                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                AccountStatusMessage = "Accounts could not be loaded.";
                MessageBox.Show("Error loading accounts: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAccount()
        {
            string trimmedUsername = Normalize(AccountUsername);
            bool isEditMode = _editingAccountId > 0;

            if (string.IsNullOrWhiteSpace(trimmedUsername))
            {
                AccountStatusMessage = "Username is required.";
                return;
            }

            if (trimmedUsername.Length > UsernameMaxLength)
            {
                AccountStatusMessage = string.Format(
                    "Username cannot be longer than {0} characters.",
                    UsernameMaxLength);
                return;
            }

            if (!isEditMode && string.IsNullOrWhiteSpace(AccountPassword))
            {
                AccountStatusMessage = "Password is required.";
                return;
            }

            if (!isEditMode &&
                !string.IsNullOrEmpty(AccountPassword) &&
                AccountPassword.Length > PasswordMaxLength)
            {
                AccountStatusMessage = string.Format(
                    "Password cannot be longer than {0} characters.",
                    PasswordMaxLength);
                return;
            }

            if (!IsSupportedAccountRole(SelectedRole))
            {
                AccountStatusMessage = "Select a valid account role.";
                return;
            }

            if (IsLibrarianRole(SelectedRole) && SelectedAccountLibraryId <= 0)
            {
                AccountStatusMessage = "Assign the librarian to a library.";
                return;
            }

            try
            {
                bool needsSaveChanges;

                using (var db = new LibraryDbContext())
                {
                    string normalizedUsername = trimmedUsername.ToLowerInvariant();

                    var existingUsernames = db.Users
                        .Where(user => user.Id != _editingAccountId && user.Username != null)
                        .Select(user => user.Username)
                        .ToList();

                    bool usernameExists = existingUsernames.Any(username =>
                        Normalize(username).ToLowerInvariant() == normalizedUsername);

                    if (usernameExists)
                    {
                        AccountStatusMessage = "This username is already used by another account.";
                        return;
                    }

                    if (IsLibrarianRole(SelectedRole) &&
                        !db.Libraries.Any(library => library.Id == SelectedAccountLibraryId))
                    {
                        AccountStatusMessage = "The selected library no longer exists.";
                        return;
                    }

                    bool savedSuccessfully;

                    if (isEditMode)
                    {
                        savedSuccessfully = SaveExistingAccount(db, trimmedUsername);
                        needsSaveChanges = false;
                    }
                    else
                    {
                        savedSuccessfully = CreateNewAccount(db, trimmedUsername);
                        needsSaveChanges = true;
                    }

                    if (!savedSuccessfully)
                    {
                        return;
                    }

                    if (needsSaveChanges)
                    {
                        db.SaveChanges();
                    }
                }

                string successMessage;

                if (isEditMode)
                {
                    successMessage = "Account updated successfully.";
                }
                else
                {
                    successMessage = "Account created successfully.";
                }

                LoadAccounts();
                ResetAccountForm();
                AccountStatusMessage = successMessage;

                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                AccountStatusMessage = "Account could not be saved.";
                MessageBox.Show("Error saving account: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CreateNewAccount(LibraryDbContext db, string trimmedUsername)
        {
            if (IsLibrarianRole(SelectedRole))
            {
                var librarian = new Model_Librarian
                {
                    Username = trimmedUsername,
                    Password = AccountPassword,
                    Role = "Librarian",
                    IsActive = IsAccountActive,
                    Library_ID = SelectedAccountLibraryId
                };

                db.Librarians.Add(librarian);
                return true;
            }

            if (IsAdministratorRole(SelectedRole))
            {
                var administrator = new Model_Administrator
                {
                    Username = trimmedUsername,
                    Password = AccountPassword,
                    Role = "Administrator",
                    IsActive = IsAccountActive,
                    Library_ID = null
                };

                db.Admins.Add(administrator);
                return true;
            }

            AccountStatusMessage = "Select a valid account role.";
            return false;
        }

        private bool SaveExistingAccount(LibraryDbContext db, string trimmedUsername)
        {
            var existingUser = db.Users.FirstOrDefault(user => user.Id == _editingAccountId);

            if (existingUser == null)
            {
                AccountStatusMessage = "The selected account no longer exists.";
                return false;
            }

            var normalizedRole = NormalizeRole(SelectedRole);

            if (_currentAdmin != null &&
                existingUser.Id == _currentAdmin.Id &&
                !IsAdministratorRole(normalizedRole))
            {
                AccountStatusMessage = "You cannot change your own administrator role.";
                return false;
            }

            if (IsAdministratorRole(existingUser.Role) &&
                !IsAdministratorRole(normalizedRole) &&
                db.Admins.Count(admin => admin.Id != existingUser.Id) == 0)
            {
                AccountStatusMessage = "At least one administrator account must remain.";
                return false;
            }

            object targetLibraryValue;

            if (IsLibrarianRole(normalizedRole))
            {
                targetLibraryValue = SelectedAccountLibraryId;
            }
            else
            {
                targetLibraryValue = DBNull.Value;
            }

            string passwordToSave = existingUser.Password;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.Database.ExecuteSqlCommand(
                        @"UPDATE [dbo].[Users]
                          SET [Username] = @p0,
                              [Password] = @p1,
                              [Role] = @p2,
                              [IsActive] = @p3,
                              [Library_ID] = @p4
                          WHERE [Id] = @p5",
                        trimmedUsername,
                        passwordToSave,
                        normalizedRole,
                        IsAccountActive,
                        targetLibraryValue,
                        _editingAccountId);

                    if (IsLibrarianRole(normalizedRole))
                    {
                        db.Database.ExecuteSqlCommand(
                            @"DELETE FROM [dbo].[Admins]
                              WHERE [Id] = @p0",
                            _editingAccountId);

                        db.Database.ExecuteSqlCommand(
                            @"IF NOT EXISTS (SELECT 1 FROM [dbo].[Librarians] WHERE [Id] = @p0)
                              BEGIN
                                  INSERT INTO [dbo].[Librarians] ([Id]) VALUES (@p0)
                              END",
                            _editingAccountId);
                    }
                    else if (IsAdministratorRole(normalizedRole))
                    {
                        db.Database.ExecuteSqlCommand(
                            @"DELETE FROM [dbo].[Librarians]
                              WHERE [Id] = @p0",
                            _editingAccountId);

                        db.Database.ExecuteSqlCommand(
                            @"IF NOT EXISTS (SELECT 1 FROM [dbo].[Admins] WHERE [Id] = @p0)
                              BEGIN
                                  INSERT INTO [dbo].[Admins] ([Id]) VALUES (@p0)
                              END",
                            _editingAccountId);
                    }
                    else
                    {
                        AccountStatusMessage = "Select a valid account role.";
                        transaction.Rollback();
                        return false;
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void StartEditingSelectedAccount(Model_User accountFromCommand)
        {
            Model_User accountToEdit;

            if (accountFromCommand != null)
            {
                accountToEdit = accountFromCommand;
            }
            else
            {
                accountToEdit = SelectedAccount;
            }

            if (accountToEdit == null)
            {
                AccountStatusMessage = "Select an account to edit.";
                return;
            }

            SelectedAccount = accountToEdit;

            _editingAccountId = accountToEdit.Id;
            AccountUsername = accountToEdit.Username;
            AccountPassword = string.Empty;
            SelectedRole = NormalizeRole(accountToEdit.Role);

            if (accountToEdit.Library_ID.HasValue)
            {
                SelectedAccountLibraryId = accountToEdit.Library_ID.Value;
            }
            else
            {
                SelectedAccountLibraryId = 0;
            }

            IsAccountActive = accountToEdit.IsActive;
            AccountStatusMessage = string.Format("Editing account: {0}", accountToEdit.Username);

            OnPropertyChanged(nameof(IsAccountEditMode));
            OnPropertyChanged(nameof(IsPasswordEnabled));
            OnPropertyChanged(nameof(AccountFormTitle));
            OnPropertyChanged(nameof(IsLibrarySelectionRequired));

            CommandManager.InvalidateRequerySuggested();
        }

        private void ResetSelectedAccountPassword(Model_User accountFromCommand)
        {
            Model_User accountToReset;

            if (accountFromCommand != null)
            {
                accountToReset = accountFromCommand;
            }
            else
            {
                accountToReset = SelectedAccount;
            }

            if (accountToReset == null)
            {
                AccountStatusMessage = "Select an account to reset password.";
                return;
            }

            SelectedAccount = accountToReset;

            var confirmation = MessageBox.Show(
                string.Format("Generate a password reset code for \"{0}\"?", accountToReset.Username),
                "Confirm Password Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                AccountStatusMessage = "Password reset operation was cancelled.";
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    var user = db.Users.FirstOrDefault(item => item.Id == accountToReset.Id);

                    if (user == null)
                    {
                        AccountStatusMessage = "The selected account no longer exists.";
                        return;
                    }

                    string resetCode = GeneratePasswordResetCode();
                    DateTime expiresAt = DateTime.UtcNow.AddMinutes(PasswordResetCodeExpirationMinutes);

                    user.MustChangePassword = true;
                    user.PasswordResetCode = resetCode;
                    user.PasswordResetCodeExpiresAt = expiresAt;

                    db.SaveChanges();

                    accountToReset.MustChangePassword = true;
                    accountToReset.PasswordResetCode = resetCode;
                    accountToReset.PasswordResetCodeExpiresAt = expiresAt;

                    AccountStatusMessage = string.Format(
                        "Password reset code generated for {0}. The code expires in {1} minutes.",
                        user.Username,
                        PasswordResetCodeExpirationMinutes);

                    MessageBox.Show(
                        string.Format(
                            "Reset code for {0}: {1}\n\nThis code expires in {2} minutes.",
                            user.Username,
                            resetCode,
                            PasswordResetCodeExpirationMinutes),
                        "Password Reset Code",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                AccountStatusMessage = "Password reset code could not be generated.";

                MessageBox.Show(
                    "Error generating password reset code: " + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GeneratePasswordResetCode()
        {
            const string allowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            char[] code = new char[PasswordResetCodeLength];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[1];

                int index = 0;

                while (index < PasswordResetCodeLength)
                {
                    randomNumberGenerator.GetBytes(buffer);

                    int value = buffer[0];
                    int maxValidValue = byte.MaxValue - ((byte.MaxValue + 1) % allowedCharacters.Length);

                    if (value > maxValidValue)
                    {
                        continue;
                    }

                    code[index] = allowedCharacters[value % allowedCharacters.Length];
                    index++;
                }
            }

            return new string(code);
        }

        private void DeleteSelectedAccount(Model_User accountFromCommand)
        {
            Model_User accountToDelete;

            if (accountFromCommand != null)
            {
                accountToDelete = accountFromCommand;
            }
            else
            {
                accountToDelete = SelectedAccount;
            }

            if (accountToDelete == null)
            {
                AccountStatusMessage = "Select an account to delete.";
                return;
            }

            SelectedAccount = accountToDelete;

            var confirmation = MessageBox.Show(
                string.Format("Are you sure you want to delete the account \"{0}\"?", accountToDelete.Username),
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                AccountStatusMessage = "Delete operation was cancelled.";
                return;
            }

            if (_currentAdmin != null && accountToDelete.Id == _currentAdmin.Id)
            {
                AccountStatusMessage = "You cannot delete your own administrator account.";
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    if (IsLibrarianRole(accountToDelete.Role))
                    {
                        var librarian = db.Librarians.FirstOrDefault(item => item.Id == accountToDelete.Id);

                        if (librarian == null)
                        {
                            AccountStatusMessage = "The selected account no longer exists.";
                            return;
                        }

                        db.Librarians.Remove(librarian);
                    }
                    else if (IsAdministratorRole(accountToDelete.Role))
                    {
                        var administrator = db.Admins.FirstOrDefault(item => item.Id == accountToDelete.Id);

                        if (administrator == null)
                        {
                            AccountStatusMessage = "The selected account no longer exists.";
                            return;
                        }

                        bool isLastAdministrator = db.Admins.Count(admin => admin.Id != accountToDelete.Id) == 0;

                        if (isLastAdministrator)
                        {
                            AccountStatusMessage = "At least one administrator account must remain.";
                            return;
                        }

                        db.Admins.Remove(administrator);
                    }
                    else
                    {
                        AccountStatusMessage = "The selected account has an unsupported role.";
                        return;
                    }

                    db.SaveChanges();
                }

                LoadAccounts();
                ResetAccountForm();
                AccountStatusMessage = "Account deleted successfully.";

                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                AccountStatusMessage = "Account could not be deleted.";
                MessageBox.Show("Error deleting account: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetAccountForm()
        {
            _editingAccountId = 0;
            AccountUsername = string.Empty;
            AccountPassword = string.Empty;
            SelectedRole = "Librarian";
            SelectedAccountLibraryId = 0;
            IsAccountActive = true;
            SelectedAccount = null;

            OnPropertyChanged(nameof(IsAccountEditMode));
            OnPropertyChanged(nameof(IsPasswordEnabled));
            OnPropertyChanged(nameof(AccountFormTitle));
            OnPropertyChanged(nameof(IsLibrarySelectionRequired));

            CommandManager.InvalidateRequerySuggested();
        }

        private string Normalize(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Trim();
        }

        private string NormalizeRole(string role)
        {
            if (IsAdministratorRole(role))
            {
                return "Administrator";
            }

            if (IsLibrarianRole(role))
            {
                return "Librarian";
            }

            if (role == null)
            {
                return string.Empty;
            }

            return role;
        }

        private string NormalizeFilterRole(string role)
        {
            if (role == null)
            {
                return "All";
            }

            if (string.Equals(role.Trim(), "All", StringComparison.OrdinalIgnoreCase))
            {
                return "All";
            }

            return NormalizeRole(role);
        }

        private bool IsSupportedAccountRole(string role)
        {
            return IsAdministratorRole(role) || IsLibrarianRole(role);
        }

        private bool IsAdministratorRole(string role)
        {
            if (role == null)
            {
                return false;
            }

            return string.Equals(role.Trim(), "Administrator", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsLibrarianRole(string role)
        {
            if (role == null)
            {
                return false;
            }

            return string.Equals(role.Trim(), "Librarian", StringComparison.OrdinalIgnoreCase);
        }
    }
}