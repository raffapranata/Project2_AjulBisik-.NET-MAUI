using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjulBisik_Hutspeng.Repositories;
using AjulBisik_Hutspeng.Services;
using Microsoft.Maui.Controls;

namespace AjulBisik_Hutspeng.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isPasswordHidden = true;

        public LoginViewModel(UserRepository userRepository, AuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        [RelayCommand]
        private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Email dan Password tidak boleh kosong.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var user = await _userRepository.GetByEmailAsync(Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
                {
                    await Shell.Current.DisplayAlert("Error", "Email atau Password salah.", "OK");
                    return;
                }

                _authService.Login(user.Id);
                await Shell.Current.GoToAsync($"//Home");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Gagal login: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToRegister()
        {
            await Shell.Current.GoToAsync($"//Register");
        }

        [RelayCommand]
        private async Task GoToForgotPassword()
        {
            await Shell.Current.GoToAsync($"ForgotPasswordPage");
        }
    }
}
