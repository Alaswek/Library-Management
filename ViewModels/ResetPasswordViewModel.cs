using System.Configuration;
using GenericUi.Commands;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.MVVM;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Input;

namespace LibraryManagement.ViewModels
{
    public class ResetPasswordViewModel : ViewModelBase
    {
        private string _username;
        private string _resetCode;
        private string _newPassword;
        private string _confirmPassword;
        private string _statusMessage;
        private bool _passwordResetSuccessfully;

        private string _adminRecoveryUsername;
        private string _adminRecoveryKey;
        private string _adminRecoveryStatusMessage;

        private const int PasswordMinLength = 6;
        private const int PasswordMaxLength = 255;
        private const int PasswordResetCodeLength = 6;
        private const int PasswordResetCodeExpirationMinutes = 15;

        public ResetPasswordViewModel()
        {
            ResetPasswordCommand = new RelayCommand(_ => ResetPassword());
            AdminRecoveryCommand = new RelayCommand(_ => GenerateAdminRecoveryResetCode());

            Username = string.Empty;
            ResetCode = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            StatusMessage = "Enter your username, reset code and new password.";

            AdminRecoveryUsername = string.Empty;
            AdminRecoveryKey = string.Empty;
            AdminRecoveryStatusMessage = "Enter the administrator username and recovery key.";
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public string ResetCode
        {
            get { return _resetCode; }
            set { SetProperty(ref _resetCode, value); }
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

        public bool PasswordResetSuccessfully
        {
            get { return _passwordResetSuccessfully; }
            private set { SetProperty(ref _passwordResetSuccessfully, value); }
        }

        public string AdminRecoveryUsername
        {
            get { return _adminRecoveryUsername; }
            set { SetProperty(ref _adminRecoveryUsername, value); }
        }

        public string AdminRecoveryKey
        {
            get { return _adminRecoveryKey; }
            set { SetProperty(ref _adminRecoveryKey, value); }
        }

        public string AdminRecoveryStatusMessage
        {
            get { return _adminRecoveryStatusMessage; }
            set { SetProperty(ref _adminRecoveryStatusMessage, value); }
        }

        public ICommand ResetPasswordCommand { get; private set; }
        public ICommand AdminRecoveryCommand { get; private set; }

        private void ResetPassword()
        {
            PasswordResetSuccessfully = false;

            if (!ValidateInput())
            {
                return;
            }

            try
            {
                string trimmedUsername = Normalize(Username);
                string trimmedResetCode = Normalize(ResetCode);
                string normalizedUsername = trimmedUsername.ToLowerInvariant();

                using (var db = new LibraryDbContext())
                {
                    Model_User matchingUser = db.Users.FirstOrDefault(account =>
                        account.Username != null &&
                        account.Username.ToLower() == normalizedUsername);

                    if (matchingUser == null)
                    {
                        StatusMessage = "User account was not found.";
                        return;
                    }

                    if (!matchingUser.MustChangePassword)
                    {
                        StatusMessage = "There is no active password reset request for this account.";
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(matchingUser.PasswordResetCode))
                    {
                        StatusMessage = "There is no valid reset code for this account.";
                        return;
                    }

                    if (!matchingUser.PasswordResetCodeExpiresAt.HasValue)
                    {
                        StatusMessage = "The reset code expiration date is missing.";
                        return;
                    }

                    if (matchingUser.PasswordResetCodeExpiresAt.Value <= DateTime.UtcNow)
                    {
                        matchingUser.MustChangePassword = false;
                        matchingUser.PasswordResetCode = null;
                        matchingUser.PasswordResetCodeExpiresAt = null;
                        db.SaveChanges();

                        StatusMessage = "The reset code has expired. Ask an administrator for a new code.";
                        return;
                    }

                    if (!string.Equals(
                        Normalize(matchingUser.PasswordResetCode),
                        trimmedResetCode,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        StatusMessage = "The reset code is incorrect.";
                        return;
                    }

                    if (matchingUser.Password == NewPassword)
                    {
                        StatusMessage = "New password must be different from the current password.";
                        return;
                    }

                    matchingUser.Password = NewPassword;
                    matchingUser.MustChangePassword = false;
                    matchingUser.PasswordResetCode = null;
                    matchingUser.PasswordResetCodeExpiresAt = null;

                    db.SaveChanges();
                }

                ClearFields();

                PasswordResetSuccessfully = true;
                StatusMessage = "Password was reset successfully.";
            }
            catch (Exception)
            {
                StatusMessage = "Password could not be reset.";
            }
        }

        private void GenerateAdminRecoveryResetCode()
        {
            if (string.IsNullOrWhiteSpace(AdminRecoveryUsername))
            {
                AdminRecoveryStatusMessage = "Username is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(AdminRecoveryKey))
            {
                AdminRecoveryStatusMessage = "Recovery key is required.";
                return;
            }

            string configuredRecoveryKey = Properties.Settings.Default.AdminRecoveryKey;

            if (string.IsNullOrWhiteSpace(configuredRecoveryKey))
            {
                AdminRecoveryStatusMessage = "Admin recovery key is not configured.";
                return;
            }

            if (!string.Equals(
                Normalize(AdminRecoveryKey),
                Normalize(configuredRecoveryKey),
                StringComparison.Ordinal))
            {
                AdminRecoveryStatusMessage = "Recovery key is incorrect.";
                return;
            }

            try
            {
                string trimmedUsername = Normalize(AdminRecoveryUsername);
                string normalizedUsername = trimmedUsername.ToLowerInvariant();

                using (var db = new LibraryDbContext())
                {
                    Model_User adminAccount = db.Users.FirstOrDefault(account =>
                        account.Username != null &&
                        account.Username.ToLower() == normalizedUsername);

                    if (adminAccount == null)
                    {
                        AdminRecoveryStatusMessage = "Administrator account was not found.";
                        return;
                    }

                    if (!IsAdministratorRole(adminAccount.Role))
                    {
                        AdminRecoveryStatusMessage = "This recovery option is only for administrator accounts.";
                        return;
                    }

                    string resetCode = GeneratePasswordResetCode();
                    DateTime expiresAt = DateTime.UtcNow.AddMinutes(PasswordResetCodeExpirationMinutes);

                    adminAccount.MustChangePassword = true;
                    adminAccount.PasswordResetCode = resetCode;
                    adminAccount.PasswordResetCodeExpiresAt = expiresAt;

                    db.SaveChanges();

                    AdminRecoveryStatusMessage = string.Format(
                        "Admin reset code generated: {0}. It expires in {1} minutes.",
                        resetCode,
                        PasswordResetCodeExpirationMinutes);
                }
            }
            catch (Exception)
            {
                AdminRecoveryStatusMessage = "Admin recovery could not be completed.";
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                StatusMessage = "Username is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ResetCode))
            {
                StatusMessage = "Reset code is required.";
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

            return true;
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

        private void ClearFields()
        {
            Username = string.Empty;
            ResetCode = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        private bool IsAdministratorRole(string role)
        {
            if (role == null)
            {
                return false;
            }

            return string.Equals(role.Trim(), "Administrator", StringComparison.OrdinalIgnoreCase);
        }

        private string Normalize(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Trim();
        }
    }
}