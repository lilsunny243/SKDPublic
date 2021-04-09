using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using DonorClient.Views;
using Plugin.FirebasePushNotification;
using SKD.Common.Droid;
using SKD.Common.Themes;
using TouchEffect.Android;
using Xamarin.Facebook;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Intent = Android.Content.Intent;
using ACDelegate = AndroidX.AppCompat.App.AppCompatDelegate;

[assembly: Dependency(typeof(LocationFetcher))]
[assembly: ExportRenderer(typeof(DonorClient.AppShell), typeof(CustomShellRenderer))]
namespace DonorClient.Droid
{
    [Activity(Label = "SKD", Icon = "@mipmap/icon", Theme = "@style/MainTheme.Launcher", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FormsAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.MainTheme);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            FacebookSdk.ApplicationId = ""; # REDACTED
            FacebookSdk.SdkInitialize(this);
            Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Window.ClearFlags(WindowManagerFlags.TranslucentNavigation);
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            ThemeEngine.OnThemeChanged += ev =>
            {
                var chromeBackgroundColour = ev.ChromeBackgroundColour.ToAndroid();
                Window.SetStatusBarColor(chromeBackgroundColour);
                Window.SetNavigationBarColor(chromeBackgroundColour);
                SetLightnessFlags(ev.IsEffectivelyLight);
                ACDelegate.DefaultNightMode = ev.IsEffectivelyLight ? ACDelegate.ModeNightNo : ACDelegate.ModeNightYes; 
            };

            AppShell.LoginAppearing += () =>
            {
                SetLightnessFlags(false);
                Window.AddFlags(WindowManagerFlags.LayoutNoLimits);
            };
            LoginPage.Animating += () => RestoreLightnessFlags();
            AppShell.LoginDisappearing += () =>
            {
                RestoreLightnessFlags();
                Window.ClearFlags(WindowManagerFlags.LayoutNoLimits);
            };

            CreditCardInputView.Appearing += () => Window.AddFlags(WindowManagerFlags.Secure);
            CreditCardInputView.Disappearing += () => Window
            .ClearFlags(WindowManagerFlags.Secure);

            GooglePayService.Init(this);
            AuthenticationService.Init(this);
            SecureCustAuthService.Init(this);
            TouchEffectPreserver.Preserve();
            Xamarin.Forms.Forms.SetFlags("IndicatorView_Experimental", "AppTheme_Experimental", "Expander_Experimental", "Shapes_Experimental", "Brush_Experimental", "SwipeView_Experimental");
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true); 
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            AiForms.Dialogs.Dialogs.Init(this);

            LoadApplication(new App());
            FirebasePushNotificationManager.ProcessIntent(this, Intent);
        }

        private int previousLightnessFlags;
        private void SetLightnessFlags(bool isLight)
        {
            previousLightnessFlags = (int)Window.DecorView.SystemUiVisibility & ((int)SystemUiFlags.LightNavigationBar | (int)SystemUiFlags.LightStatusBar);
            int newUiVisibility = (int)Window.DecorView.SystemUiVisibility;
            if (isLight)
                newUiVisibility |= (int)SystemUiFlags.LightStatusBar | (int)SystemUiFlags.LightNavigationBar;
            else
                newUiVisibility &= ~((int)SystemUiFlags.LightStatusBar | (int)SystemUiFlags.LightNavigationBar);
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)newUiVisibility;
        }

        private void RestoreLightnessFlags()
        {
            int newLightnessFlags = (int)Window.DecorView.SystemUiVisibility;
            newLightnessFlags |= previousLightnessFlags;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)newLightnessFlags;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            GooglePayService.OnActivityResult?.Invoke(requestCode, resultCode, data);
            AuthenticationService.OnActivityResult?.Invoke(requestCode, resultCode, data);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Xamarin.Essentials.Platform.OnResume();
        }


        [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
        [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "")] # REDACTED
        public class FacebookCustomTabActivity : CustomTabActivity { }

        [Activity(Label = "Street Kids Direct", Icon = "@mipmap/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
        public class FacebookActivity : Xamarin.Facebook.FacebookActivity { }
    }

}
