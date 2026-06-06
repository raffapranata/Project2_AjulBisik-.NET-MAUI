using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.Views;
using AjulBisik_Hutspeng.Services;

namespace AjulBisik_Hutspeng
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(VerifyOtpPage), typeof(VerifyOtpPage));
            Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
            Routing.RegisterRoute(nameof(MessageDetailPage), typeof(MessageDetailPage));
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
        }
    }
}
