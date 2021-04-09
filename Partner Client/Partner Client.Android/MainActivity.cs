using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using PartnerClient.Views;
using SKD.Common.Droid;
using SKD.Common.Themes;
using TouchEffect.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using ACDelegate = AndroidX.AppCompat.App.AppCompatDelegate;

[assembly: Dependency(typeof(LocationFetcher))]
[assembly: ExportRenderer(typeof(PartnerClient.AppShell), typeof(CustomShellRenderer))]
namespace PartnerClient.Droid
{
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataSchemes = new[] { "http", "https" }, DataHost = "street-kids-direct-dms.firebaseapp.com")]
    [Activity(Label = "SKD Partner", Icon = "@mipmap/icon", Theme = "@style/MainTheme.Launcher", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.MainTheme);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
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

            TouchEffectPreserver.Preserve();
            Xamarin.Forms.Forms.SetFlags("IndicatorView_Experimental", "SwipeView_Experimental", "AppTheme_Experimental", "Expander_Experimental", "Shapes_Experimental", "Brush_Experimental");
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            AiForms.Dialogs.Dialogs.Init(this);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Sharpnado.HorizontalListView.Droid.SharpnadoInitializer.Initialize();
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}