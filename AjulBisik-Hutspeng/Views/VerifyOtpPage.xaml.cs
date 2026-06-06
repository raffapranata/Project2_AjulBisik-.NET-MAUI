using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.ViewModels;

namespace AjulBisik_Hutspeng.Views
{
    public partial class VerifyOtpPage : ContentPage
    {
        public VerifyOtpPage(VerifyOtpViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
