using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.ViewModels;

namespace AjulBisik_Hutspeng.Views
{
    public partial class AccountPage : ContentPage
    {
        private readonly AccountViewModel _viewModel;

        public AccountPage(AccountViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadUserCommand.Execute(null);
        }
    }
}
