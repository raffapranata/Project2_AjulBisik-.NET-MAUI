using CommunityToolkit.Mvvm.ComponentModel;

namespace AjulBisik_Hutspeng.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;
    }
}
