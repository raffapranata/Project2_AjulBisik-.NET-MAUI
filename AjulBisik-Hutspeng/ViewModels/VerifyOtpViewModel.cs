using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.Services;
using AjulBisik_Hutspeng.Repositories;
using AjulBisik_Hutspeng.Models;

namespace AjulBisik_Hutspeng.ViewModels
{
    public partial class VerifyOtpViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly EmailService _emailService;
        private readonly OtpRepository _otpRepository;
        private readonly UserRepository _userRepository;

        [ObservableProperty]
        private string _otpCode;

        [ObservableProperty]
        private string _messageText;

        [ObservableProperty]
        private string _timerText = "02:00";

        [ObservableProperty]
        private bool _canResend = false;

        private string _email;
        private string _type;
        private string _fullName;
        private string _username;
        private string _password; // plain password during registration for DB insertion later
        private bool _isTimerRunning = false;

        public VerifyOtpViewModel(EmailService emailService, OtpRepository otpRepository, UserRepository userRepository)
        {
            _emailService = emailService;
            _otpRepository = otpRepository;
            _userRepository = userRepository;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("Email")) _email = query["Email"].ToString();
            if (query.ContainsKey("Type")) _type = query["Type"].ToString();

            if (query.ContainsKey("FullName")) _fullName = query["FullName"].ToString();
            if (query.ContainsKey("Username")) _username = query["Username"].ToString();
            if (query.ContainsKey("Password")) _password = query["Password"].ToString();

            MessageText = $"OTP telah dikirim ke {_email}. Silakan cek kotak masuk Anda.";
            
            if (!_isTimerRunning)
            {
                _ = StartCountdownAsync();
            }
        }

        private async Task StartCountdownAsync()
        {
            _isTimerRunning = true;
            CanResend = false;
            int seconds = 120; // 2 minutes

            while (seconds > 0)
            {
                int minutes = seconds / 60;
                int remainingSeconds = seconds % 60;
                TimerText = $"{minutes:D2}:{remainingSeconds:D2}";
                
                await Task.Delay(1000);
                seconds--;
            }

            TimerText = string.Empty;
            CanResend = true;
            _isTimerRunning = false;
        }

        [RelayCommand]
        private async Task VerifyOtpAsync()
        {
            if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length != 6)
            {
                await Shell.Current.DisplayAlert("Error", "Masukkan 6 digit kode OTP.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var record = await _otpRepository.GetValidOtpAsync(_email, _type);
                if (record == null || record.OtpCode != OtpCode)
                {
                    await Shell.Current.DisplayAlert("Error", "Kode OTP salah atau sudah digunakan.", "OK");
                    return;
                }

                if (DateTime.UtcNow > record.ExpiryTime)
                {
                    await Shell.Current.DisplayAlert("Error", "Kode OTP telah kadaluarsa. Silakan kirim ulang.", "OK");
                    return;
                }

                // OTP is valid
                await _otpRepository.MarkAsUsedAsync(record.Id);

                // Handle based on Type
                if (_type == "Register")
                {
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = _fullName,
                        Username = _username,
                        Email = _email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(_password),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var success = await _userRepository.CreateAsync(user);
                    if (success)
                    {
                        await Shell.Current.DisplayAlert("Sukses", "Pendaftaran berhasil, silakan masuk.", "OK");
                        // Clear stack and go to login
                        await Shell.Current.GoToAsync($"//Login");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Gagal menyimpan akun.", "OK");
                    }
                }
                else if (_type == "ForgotPassword")
                {
                    // Navigate to a new page to enter new password, pass the email
                    await Shell.Current.GoToAsync($"ForgotPasswordPage?Email={_email}&OtpVerified=true");
                }
                else if (_type == "ChangePassword")
                {
                    // Return back with verified status
                    var navParams = new Dictionary<string, object>
                    {
                        { "OtpVerified", true },
                        { "OtpType", "ChangePassword" }
                    };
                    await Shell.Current.GoToAsync("..", navParams);
                }
                else if (_type == "DeleteAccount")
                {
                    var navParams = new Dictionary<string, object>
                    {
                        { "OtpVerified", true },
                        { "OtpType", "DeleteAccount" }
                    };
                    await Shell.Current.GoToAsync("..", navParams);
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

        [RelayCommand]
        private async Task ResendOtpAsync()
        {
            if (!CanResend) return;

            IsBusy = true;
            try
            {
                var record = await _otpRepository.GetValidOtpAsync(_email, _type);
                string newOtp = _emailService.GenerateOtp();
                bool success = false;

                if (record != null)
                {
                    if (record.ResendCount >= 3)
                    {
                        await Shell.Current.DisplayAlert("Error", "Batas maksimal pengiriman ulang OTP telah tercapai. Silakan ulangi proses dari awal nanti.", "OK");
                        return;
                    }

                    DateTime newExpiry = DateTime.UtcNow.AddMinutes(5);
                    success = await _otpRepository.IncrementResendCountAsync(record.Id, newOtp, newExpiry);
                }
                else
                {
                    // Create new if none exists or all used/expired
                    var newRecord = new OtpRecord
                    {
                        Id = Guid.NewGuid(),
                        Email = _email,
                        OtpCode = newOtp,
                        Type = _type,
                        ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                        ResendCount = 0,
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    success = await _otpRepository.CreateOrUpdateOtpAsync(newRecord);
                }

                if (success)
                {
                    await _emailService.SendOtpEmailAsync(_email, newOtp, $"Kirim OTP ({_type})");
                    await Shell.Current.DisplayAlert("Info", "Kode OTP baru telah dikirim ke email Anda.", "OK");
                    _ = StartCountdownAsync();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Gagal mengirim ulang OTP: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
