using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows.Input;

namespace LibraryManagement.ViewModels
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly Model_User _currentUser;

        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _statusMessage;
        private bool _passwordChangedSuccessfully;

        private const int PasswordMinLength = 6;
        private const int PasswordMaxLength = 255;

        public ChangePasswordViewModel(Model_User currentUser)
        {
            _currentUser = currentUser;

            ChangePasswordCommand = new RelayCommand(_ => ChangePassword());

            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            StatusMessage = "Enter your current password and your new password.";
        }

        public string CurrentPassword
        {
            get { return _currentPassword; }
            set { SetProperty(ref _currentPassword, value); }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set { SetProperty(ref _newPassword, value); }
        }

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { SetProperty(ref _confirmPassword, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        public bool PasswordChangedSuccessfully
        {
            get { return _passwordChangedSuccessfully; }
            private set { SetProperty(ref _passwordChangedSuccessfully, value); }
        }

        public ICommand ChangePasswordCommand { get; private set; }

        private void ChangePassword()
        {
            PasswordChangedSuccessfully = false;

            if (!ValidateInput())
            {
                return;
            }

            try
            {
                using (var db = new LibraryDbContext())
                {
                    var user = db.Users.FirstOrDefault(item => item.Id == _currentUser.Id);

                    if (user == null)
                    {
                        StatusMessage = "User account no longer exists.";
                        return;
                    }

                    if (user.Password != CurrentPassword)
                    {
                        StatusMessage = "Current password is incorrect.";
                        return;
                    }

                    user.Password = NewPassword;
                    db.SaveChanges();

                    _currentUser.Password = NewPassword;
                }

                ClearPasswordFields();

                PasswordChangedSuccessfully = true;
                StatusMessage = "Password changed successfully.";
            }
            catch (DbEntityValidationException)
            {
                StatusMessage = "Password could not be changed because the data is not valid.";
            }
            catch (DbUpdateException)
            {
                StatusMessage = "Password could not be changed because the database update failed.";
            }
            catch (InvalidOperationException)
            {
                StatusMessage = "Password could not be changed because the database operation is invalid.";
            }
            catch (Exception)
            {
                StatusMessage = "Password could not be changed.";
            }
        }

        private bool ValidateInput()
        {
            if (_currentUser == null || _currentUser.Id <= 0)
            {
                StatusMessage = "Current user could not be identified.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                StatusMessage = "Current password is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                StatusMessage = "New password is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                StatusMessage = "Confirm password is required.";
                return false;
            }

            if (NewPassword.Length < PasswordMinLength)
            {
                StatusMessage = string.Format(
                    "New password must have at least {0} characters.",
                    PasswordMinLength);
                return false;
            }

            if (NewPassword.Length > PasswordMaxLength)
            {
                StatusMessage = string.Format(
                    "New password cannot be longer than {0} characters.",
                    PasswordMaxLength);
                return false;
            }

            if (NewPassword != NewPassword.Trim())
            {
                StatusMessage = "New password cannot start or end with spaces.";
                return false;
            }

            if (NewPassword != ConfirmPassword)
            {
                StatusMessage = "New password and confirmation password do not match.";
                return false;
            }

            if (CurrentPassword == NewPassword)
            {
                StatusMessage = "New password must be different from the current password.";
                return false;
            }

            return true;
        }

        private void ClearPasswordFields()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}