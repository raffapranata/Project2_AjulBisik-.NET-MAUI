using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.ViewModels;

namespace AjulBisik_Hutspeng.Views
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
