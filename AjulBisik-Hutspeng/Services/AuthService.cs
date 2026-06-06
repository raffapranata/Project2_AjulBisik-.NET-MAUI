using System;
using Microsoft.Maui.Storage;

namespace AjulBisik_Hutspeng.Services
{
    public class AuthService
    {
        private const string LoggedInUserIdKey = "LoggedInUserId";
        private const string HasSeenOnboardingKey = "HasSeenOnboarding";

        public bool IsLoggedIn => Preferences.ContainsKey(LoggedInUserIdKey);

        public Guid? CurrentUserId
        {
            get
            {
                var idStr = Preferences.Get(LoggedInUserIdKey, string.Empty);
                if (Guid.TryParse(idStr, out var id))
                    return id;
                return null;
            }
        }

        public void Login(Guid userId)
        {
            Preferences.Set(LoggedInUserIdKey, userId.ToString());
        }

        public void Logout()
        {
            Preferences.Remove(LoggedInUserIdKey);
        }

        public bool HasSeenOnboarding
        {
            get => Preferences.Get(HasSeenOnboardingKey, false);
            set => Preferences.Set(HasSeenOnboardingKey, value);
        }
    }
}
