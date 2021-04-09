using DonorClient.Models;
using DonorClient.Utils;
using DonorClient.ViewModels;
using Plugin.CloudFirestore;
using Plugin.FirebaseAuth;
using Plugin.FirebasePushNotification;
using SKD.Common.Models;
using SKD.Common.Themes;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using StripeConfig = Stripe.StripeConfiguration;

namespace DonorClient
{
    public partial class App : Application
    {
#if DEBUG
        public const string StripeKey = ""; # REDACTED
#else
        public const string StripeKey = ""; # REDACTED
#endif
        public const string AppleMerchantId = "merchant.org.uk.streetkidsdirect.donor";
        public const double StripeFeeCoefficient = 0.014;
        public const int StripeFeeConstant = 20;

        public App()
        {
            InitializeComponent();
            ThemeEngine.Init(this);
            StripeConfig.ApiKey = StripeKey;
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            // Set the most likely-to-be-accurate theme before user account is loaded but not interfere with login screen theming on android
            string themePreference = await SecureStorage.GetAsync("ThemePreference");
            ThemeEngine.SetTheme(string.IsNullOrEmpty(themePreference) ? Theme.System : (Theme)Enum.Parse(typeof(Theme), themePreference),
                Shell.Current.CurrentState.Location.OriginalString != "//Login");
        }

        protected override void OnSleep()
        {
            SettingsViewModel.ClearPaymentData?.Invoke();
        }

        protected override void OnResume()
        {
        }

    }
}
