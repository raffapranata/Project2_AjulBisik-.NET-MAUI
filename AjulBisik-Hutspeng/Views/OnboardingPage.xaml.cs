using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.ViewModels;

namespace AjulBisik_Hutspeng.Views
{
    public partial class OnboardingPage : ContentPage
    {
        public OnboardingPage(OnboardingViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
