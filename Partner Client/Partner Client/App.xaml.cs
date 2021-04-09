using System;
using System.Globalization;
using System.Web;
using Plugin.FirebaseAuth;
using SKD.Common.Services;
using SKD.Common.Themes;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PartnerClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Sharpnado.HorizontalListView.Initializer.Initialize(true, false);
            SKD.Common.Themes.ThemeEngine.SetTheme(SKD.Common.Themes.Theme.System);
            CrossFirebaseAuth.Current.Instance.LanguageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            // Set the most likely-to-be-accurate theme before user account is loaded but not interfere with login screen theming on android
            string themePreference = await SecureStorage.GetAsync("ThemePreference");
            ThemeEngine.SetTheme(string.IsNullOrEmpty(themePreference) ? Theme.System : (Theme)Enum.Parse(typeof(Theme), themePreference),
                Shell.Current.CurrentState.Location.OriginalString != "//Login");

            // Check photo access permission
            try
            {
                var result = await Permissions.CheckStatusAsync<Permissions.Media>();
                if (result != PermissionStatus.Granted)
                    await Permissions.RequestAsync<Permissions.Media>();
            }
            catch { } // Absorb
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            (Shell.Current as AppShell)!.ForceOnIdTokenChanged();
        }

        protected override async void OnAppLinkRequestReceived(Uri uri)
        {
            if (uri.Host == "") { # REDACTED
                var query = HttpUtility.ParseQueryString(uri.Query);
                var oobCode = query.Get("oobCode");
                var mode = query.Get("mode");
                if (mode == "verifyEmail")
                    await CrossFirebaseAuth.Current.Instance.ApplyActionCodeAsync(oobCode);
                (Shell.Current as AppShell)!.ForceOnIdTokenChanged();
            }
            base.OnAppLinkRequestReceived(uri);
        }

        public static ActionCodeSettings FirebaseActionCodeSettings = new ActionCodeSettings()
        {
            HandleCodeInApp = true,
            IosBundleId = "org.streetkidsdirect.partner",
            Url = @"https://www.streetkidsdirect.org.uk/"
        };

    }
}
