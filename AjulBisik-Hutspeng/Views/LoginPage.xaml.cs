using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.ViewModels;

namespace AjulBisik_Hutspeng.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
