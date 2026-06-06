using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.Services;
using AjulBisik_Hutspeng.Repositories;
using AjulBisik_Hutspeng.Models;

namespace AjulBisik_Hutspeng.ViewModels
{
    public partial class ForgotPasswordViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly UserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly OtpRepository _otpRepository;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _newPassword;

        [ObservableProperty]
        private string _confirmNewPassword;

        [ObservableProperty]
        private bool _isPasswordHidden = true;

        [ObservableProperty]
        private bool _isConfirmPasswordHidden = true;

        [ObservableProperty]
        private bool _isRequestingOtp = true;

        [ObservableProperty]
        private bool _isOtpVerified = false;

        public ForgotPasswordViewModel(UserRepository userRepository, EmailService emailService, OtpRepository otpRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _otpRepository = otpRepository;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("Email")) Email = query["Email"].ToString();
            if (query.ContainsKey("OtpVerified"))
            {
                IsOtpVerified = true;
                IsRequestingOtp = false;
            }
        }

        [RelayCommand]
        private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

        [RelayCommand]
        private void ToggleConfirmPassword() => IsConfirmPasswordHidden = !IsConfirmPasswordHidden;

        [RelayCommand]
        private async Task RequestOtpAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                await Shell.Current.DisplayAlert("Error", "Masukkan email yang valid.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var user = await _userRepository.GetByEmailAsync(Email);
                if (user == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Email tidak terdaftar di sistem kami.", "OK");
                    return;
                }

                string otpCode = _emailService.GenerateOtp();
                var record = new OtpRecord
                {
                    Id = Guid.NewGuid(),
                    Email = Email,
                    OtpCode = otpCode,
                    Type = "ForgotPassword",
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    ResendCount = 0,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _otpRepository.CreateOrUpdateOtpAsync(record);
                await _emailService.SendOtpEmailAsync(Email, otpCode, "Lupa Password");

                var navParams = new Dictionary<string, object>
                {
                    { "Email", Email },
                    { "Type", "ForgotPassword" }
                };
                await Shell.Current.GoToAsync("VerifyOtpPage", navParams);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Gagal memproses: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                await Shell.Current.DisplayAlert("Error", "Password baru tidak boleh kosong.", "OK");
                return;
            }

            if (NewPassword.Length < 8 || !Regex.IsMatch(NewPassword, @"[A-Za-z]") || !Regex.IsMatch(NewPassword, @"[0-9]"))
            {
                await Shell.Current.DisplayAlert("Error", "Password minimal 8 karakter dan harus mengandung kombinasi huruf dan angka.", "OK");
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Konfirmasi password tidak cocok.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var user = await _userRepository.GetByEmailAsync(Email);
                if (user != null)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                    var success = await _userRepository.UpdateAsync(user);
                    if (success)
                    {
                        await Shell.Current.DisplayAlert("Sukses", "Password berhasil diubah. Silakan login kembali.", "OK");
                        await Shell.Current.GoToAsync("//Login");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Gagal mengubah password.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Terjadi kesalahan: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
