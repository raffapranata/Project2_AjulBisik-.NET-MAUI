using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjulBisik_Hutspeng.Models;
using AjulBisik_Hutspeng.Services;
using Microsoft.Maui.Controls;

namespace AjulBisik_Hutspeng.ViewModels
{
    public partial class OnboardingViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private ObservableCollection<OnboardingItem> _items;

        [ObservableProperty]
        private int _position;

        public OnboardingViewModel(AuthService authService)
        {
            _authService = authService;
            Items = new ObservableCollection<OnboardingItem>
            {
                new OnboardingItem { Title = "Kirim Pesan Tanpa Jejak", Description = "Kirim pesan anonim ke teman-temanmu secara rahasia dan aman.", ImageUrl = "onboard1.jpg" },
                new OnboardingItem { Title = "Anti Spam", Description = "Terlindungi dari spam. Hanya 1 pesan setiap 5 menit.", ImageUrl = "onboard2.jpg" },
                new OnboardingItem { Title = "Aman & Nyaman", Description = "Bebas berekspresi tanpa takut di-judge. Yuk, mulai sekarang!", ImageUrl = "onboard3.jpg" }
            };
        }

        [RelayCommand]
        private void Next()
        {
            if (Position < Items.Count - 1)
                Position++;
            else
                FinishOnboarding();
        }

        [RelayCommand]
        private async void FinishOnboarding()
        {
            _authService.HasSeenOnboarding = true;
            await Shell.Current.GoToAsync($"//Login");
        }
    }
}
