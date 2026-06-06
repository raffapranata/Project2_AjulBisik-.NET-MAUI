using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.ViewModels;

namespace AjulBisik_Hutspeng.Views
{
    public partial class MessageDetailPage : ContentPage
    {
        public MessageDetailPage(MessageDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}