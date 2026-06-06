using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjulBisik_Hutspeng.Models;
using AjulBisik_Hutspeng.Repositories;
using AjulBisik_Hutspeng.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace AjulBisik_Hutspeng.ViewModels
{
    public partial class AccountViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;
        private readonly EmailService _emailService;
        private readonly OtpRepository _otpRepository;

        [ObservableProperty]
        private User _currentUser;

        [ObservableProperty]
        private ImageSource _profilePhotoSource;

        [ObservableProperty]
        private string _oldPassword;

        [ObservableProperty]
        private string _newPassword;

        [ObservableProperty]
        private string _confirmNewPassword;

        [ObservableProperty]
        private string _deleteConfirmation;

        [ObservableProperty]
        private bool _isOldPasswordHidden = true;

        [ObservableProperty]
        private bool _isNewPasswordHidden = true;

        [ObservableProperty]
        private bool _isConfirmNewPasswordHidden = true;

        [ObservableProperty]
        private int _selectedThemeIndex;

        [ObservableProperty]
        private bool _isSettingsTab = true;

        [ObservableProperty]
        private bool _isMenuVisible = true;

        private string _pendingNewPassword;

        public AccountViewModel(UserRepository userRepository, AuthService authService, EmailService emailService, OtpRepository otpRepository)
        {
            _userRepository = userRepository;
            _authService = authService;
            _emailService = emailService;
            _otpRepository = otpRepository;
            
            _selectedThemeIndex = Preferences.Default.Get("AppTheme", 0);
        }

        [RelayCommand]
        private void ShowSettings()
        {
            IsSettingsTab = true;
            IsMenuVisible = false;
        }

        [RelayCommand]
        private void ShowSecurity()
        {
            IsSettingsTab = false;
            IsMenuVisible = false;
        }

        [RelayCommand]
        private void BackToMenu()
        {
            IsMenuVisible = true;
        }

        partial void OnSelectedThemeIndexChanged(int value)
        {
            Preferences.Default.Set("AppTheme", value);
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = value switch
                {
                    1 => AppTheme.Light,
                    2 => AppTheme.Dark,
                    _ => AppTheme.Unspecified
                };
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("OtpVerified") && (bool)query["OtpVerified"] == true)
            {
                if (query.ContainsKey("OtpType"))
                {
                    string type = query["OtpType"].ToString();
                    if (type == "ChangePassword" && !string.IsNullOrEmpty(_pendingNewPassword))
                    {
                        await ExecuteChangePasswordAsync();
                    }
                    else if (type == "DeleteAccount")
                    {
                        await ExecuteDeleteAccountAsync();
                    }
                }
            }
        }

        [RelayCommand]
        public async Task LoadUserAsync()
        {
            var userId = _authService.CurrentUserId;
            if (userId != null)
            {
                IsBusy = true;
                CurrentUser = await _userRepository.GetByIdAsync(userId.Value);
                UpdateProfilePhotoSource();
                IsBusy = false;
            }
        }

        private void UpdateProfilePhotoSource()
        {
            if (CurrentUser?.ProfilePhotoData != null)
            {
                ProfilePhotoSource = ImageSource.FromStream(() => new MemoryStream(CurrentUser.ProfilePhotoData));
            }
            else
            {
                ProfilePhotoSource = "ic_account.png";
            }
        }

        [RelayCommand]
        private async Task ChangePhotoAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync();
                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    var bytes = memoryStream.ToArray();

                    CurrentUser.ProfilePhotoData = bytes;
                    bool success = await _userRepository.UpdateAsync(CurrentUser);
                    if (success)
                    {
                        UpdateProfilePhotoSource();
                        await Shell.Current.DisplayAlert("Sukses", "Foto profil berhasil diperbarui.", "OK");
                    }
                }
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Error", "Gagal mengunggah foto.", "OK");
            }
        }

        [RelayCommand]
        private async Task UpdateProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.FullName) || string.IsNullOrWhiteSpace(CurrentUser.Username) || string.IsNullOrWhiteSpace(CurrentUser.Email))
            {
                await Shell.Current.DisplayAlert("Error", "Nama, Username, dan Email tidak boleh kosong.", "OK");
                return;
            }

            if (!CurrentUser.Email.Contains("@") || !CurrentUser.Email.Contains("."))
            {
                await Shell.Current.DisplayAlert("Error", "Format email tidak valid.", "OK");
                return;
            }

            if (!Regex.IsMatch(CurrentUser.Username, @"^[a-z0-9_\.]+$"))
            {
                await Shell.Current.DisplayAlert("Error", "Username hanya boleh huruf kecil, angka, underscore, dan titik.", "OK");
                return;
            }

            var existingUser = await _userRepository.GetByUsernameAsync(CurrentUser.Username);
            if (existingUser != null && existingUser.Id != CurrentUser.Id)
            {
                await Shell.Current.DisplayAlert("Error", "Username sudah digunakan.", "OK");
                return;
            }

            var existingEmail = await _userRepository.GetByEmailAsync(CurrentUser.Email);
            if (existingEmail != null && existingEmail.Id != CurrentUser.Id)
            {
                await Shell.Current.DisplayAlert("Error", "Email sudah digunakan.", "OK");
                return;
            }

            bool success = await _userRepository.UpdateAsync(CurrentUser);
            if (success)
                await Shell.Current.DisplayAlert("Sukses", "Profil berhasil diperbarui.", "OK");
            else
                await Shell.Current.DisplayAlert("Error", "Gagal memperbarui profil.", "OK");
        }

        [RelayCommand]
        private void ToggleOldPassword() => IsOldPasswordHidden = !IsOldPasswordHidden;

        [RelayCommand]
        private void ToggleNewPassword() => IsNewPasswordHidden = !IsNewPasswordHidden;

        [RelayCommand]
        private void ToggleConfirmNewPassword() => IsConfirmNewPasswordHidden = !IsConfirmNewPasswordHidden;

        [RelayCommand]
        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                await Shell.Current.DisplayAlert("Error", "Semua kolom kata sandi wajib diisi.", "OK");
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Kata sandi baru dan konfirmasi tidak cocok.", "OK");
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(OldPassword, CurrentUser.PasswordHash))
            {
                await Shell.Current.DisplayAlert("Error", "Kata sandi lama salah.", "OK");
                return;
            }

            if (NewPassword.Length < 8 || !Regex.IsMatch(NewPassword, @"[A-Za-z]") || !Regex.IsMatch(NewPassword, @"[0-9]"))
            {
                await Shell.Current.DisplayAlert("Error", "Password baru minimal 8 karakter dan kombinasi huruf dan angka.", "OK");
                return;
            }

            _pendingNewPassword = NewPassword;
            await RequestOtpAsync("ChangePassword", "Ubah Kata Sandi");
        }

        private async Task ExecuteChangePasswordAsync()
        {
            if (CurrentUser != null && !string.IsNullOrEmpty(_pendingNewPassword))
            {
                CurrentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(_pendingNewPassword);
                bool success = await _userRepository.UpdateAsync(CurrentUser);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Sukses", "Kata sandi berhasil diubah.", "OK");
                    OldPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmNewPassword = string.Empty;
                    _pendingNewPassword = null;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Gagal mengubah kata sandi.", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task DeleteAccountAsync()
        {
            if (DeleteConfirmation != "hapus_akun")
            {
                await Shell.Current.DisplayAlert("Error", "Ketik 'hapus_akun' untuk konfirmasi.", "OK");
                return;
            }

            bool confirmed = await Shell.Current.DisplayAlert("Konfirmasi", "Apakah Anda yakin ingin menghapus akun secara permanen?", "Ya", "Batal");
            if (confirmed)
            {
                await RequestOtpAsync("DeleteAccount", "Hapus Akun");
            }
        }

        private async Task ExecuteDeleteAccountAsync()
        {
            if (CurrentUser != null)
            {
                bool success = await _userRepository.DeleteAsync(CurrentUser.Id);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Sukses", "Akun berhasil dihapus.", "OK");
                    Logout();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Gagal menghapus akun.", "OK");
                }
            }
        }

        private async Task RequestOtpAsync(string type, string reason)
        {
            IsBusy = true;
            try
            {
                string otpCode = _emailService.GenerateOtp();
                var record = new OtpRecord
                {
                    Id = Guid.NewGuid(),
                    Email = CurrentUser.Email,
                    OtpCode = otpCode,
                    Type = type,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    ResendCount = 0,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _otpRepository.CreateOrUpdateOtpAsync(record);
                await _emailService.SendOtpEmailAsync(CurrentUser.Email, otpCode, reason);

                var navParams = new Dictionary<string, object>
                {
                    { "Email", CurrentUser.Email },
                    { "Type", type }
                };
                await Shell.Current.GoToAsync("VerifyOtpPage", navParams);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Gagal meminta OTP: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            Shell.Current.GoToAsync($"//Login");
        }
    }
}
