using Microsoft.Maui.Controls;
using AjulBisik_Hutspeng.Services;
using AjulBisik_Hutspeng.Views;
using System.Threading.Tasks;

namespace AjulBisik_Hutspeng
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        private readonly DatabaseService _dbService;

        public App(AuthService authService, DatabaseService dbService)
        {
            InitializeComponent();

            _authService = authService;
            _dbService = dbService;

            // Apply Theme
            int themeIndex = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", 0);
            UserAppTheme = themeIndex switch
            {
                1 => AppTheme.Light,
                2 => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Initialize DB
            await _dbService.InitializeDatabaseAsync();

            // Determine routing
            if (_authService.IsLoggedIn)
            {
                await Shell.Current.GoToAsync("//Home");
            }
            else
            {
                // Sesuai permintaan, jalankan halaman Onboarding terlebih dahulu
                await Shell.Current.GoToAsync("//Onboarding");
            }
        }
    }
}