using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjulBisik_Hutspeng.Models;
using AjulBisik_Hutspeng.Repositories;
using AjulBisik_Hutspeng.Services;
using Microsoft.Maui.Controls;

namespace AjulBisik_Hutspeng.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly OtpRepository _otpRepository;

        [ObservableProperty]
        private string _fullName;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private bool _isPasswordHidden = true;

        [ObservableProperty]
        private bool _isConfirmPasswordHidden = true;

        public RegisterViewModel(UserRepository userRepository, EmailService emailService, OtpRepository otpRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _otpRepository = otpRepository;
        }

        [RelayCommand]
        private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

        [RelayCommand]
        private void ToggleConfirmPassword() => IsConfirmPasswordHidden = !IsConfirmPasswordHidden;

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || 
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Shell.Current.DisplayAlert("Error", "Semua kolom wajib diisi.", "OK");
                return;
            }

            if (!Email.Contains("@") || !Email.Contains("."))
            {
                await Shell.Current.DisplayAlert("Error", "Format email tidak valid.", "OK");
                return;
            }

            if (!Regex.IsMatch(Username, @"^(?=.*[0-9])[a-z0-9_\.]+$"))
            {
                await Shell.Current.DisplayAlert("Error", "Username wajib mengandung angka dan hanya boleh memakai huruf kecil, garis bawah (_), atau titik (.). Spasi tidak diizinkan.", "OK");
                return;
            }

            if (Password.Length < 8 || !Regex.IsMatch(Password, @"[A-Za-z]") || !Regex.IsMatch(Password, @"[0-9]"))
            {
                await Shell.Current.DisplayAlert("Error", "Password minimal 8 karakter dan harus mengandung kombinasi huruf dan angka.", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Konfirmasi password tidak cocok.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var existingUser = await _userRepository.GetByUsernameAsync(Username);
                if (existingUser != null)
                {
                    await Shell.Current.DisplayAlert("Error", "Username sudah digunakan.", "OK");
                    return;
                }

                var existingEmail = await _userRepository.GetByEmailAsync(Email);
                if (existingEmail != null)
                {
                    await Shell.Current.DisplayAlert("Error", "Email sudah terdaftar.", "OK");
                    return;
                }

                // Generate OTP and save to DB
                string otpCode = _emailService.GenerateOtp();
                var record = new OtpRecord
                {
                    Id = Guid.NewGuid(),
                    Email = Email,
                    OtpCode = otpCode,
                    Type = "Register",
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    ResendCount = 0,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _otpRepository.CreateOrUpdateOtpAsync(record);
                
                // Send email
                await _emailService.SendOtpEmailAsync(Email, otpCode, "Pendaftaran Akun Baru");

                // Navigate to Verify OTP Page with user data
                var navParams = new Dictionary<string, object>
                {
                    { "Email", Email },
                    { "Type", "Register" },
                    { "FullName", FullName },
                    { "Username", Username },
                    { "Password", Password }
                };

                await Shell.Current.GoToAsync("VerifyOtpPage", navParams);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Gagal memproses pendaftaran: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToLogin()
        {
            await Shell.Current.GoToAsync($"//Login");
        }
    }
}
