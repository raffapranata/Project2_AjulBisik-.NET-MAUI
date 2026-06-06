using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.ViewModels;

namespace AjulBisik_Hutspeng.Views
{
    public partial class SendMessagePage : ContentPage
    {
        public SendMessagePage(SendMessageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
