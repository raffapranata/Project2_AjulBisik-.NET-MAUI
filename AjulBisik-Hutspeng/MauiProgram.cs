using Microsoft.Extensions.Logging;
using AjulBisik_Hutspeng.Services;
using AjulBisik_Hutspeng.Repositories;
using AjulBisik_Hutspeng.ViewModels;
using AjulBisik_Hutspeng.Views;

namespace AjulBisik_Hutspeng
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register Services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<EmailService>();
            builder.Services.AddSingleton<ShareService>();

            // Register Repositories
            builder.Services.AddSingleton<UserRepository>();
            builder.Services.AddSingleton<MessageRepository>();
            builder.Services.AddSingleton<OtpRepository>();

            // Register ViewModels
            builder.Services.AddTransient<OnboardingViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<VerifyOtpViewModel>();
            builder.Services.AddTransient<ForgotPasswordViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<SendMessageViewModel>();
            builder.Services.AddTransient<AccountViewModel>();
            builder.Services.AddTransient<MessageDetailViewModel>();

            // Register Views
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<VerifyOtpPage>();
            builder.Services.AddTransient<ForgotPasswordPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<SendMessagePage>();
            builder.Services.AddTransient<AccountPage>();
            builder.Services.AddTransient<MessageDetailPage>();

            return builder.Build();
        }
    }
}
