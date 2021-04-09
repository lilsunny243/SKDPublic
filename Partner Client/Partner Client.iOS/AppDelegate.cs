using Foundation;
using SKD.Common.iOS;
using SKD.Common.Themes;
using TouchEffect.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationFetcher))]
[assembly: ExportRenderer(typeof(PartnerClient.AppShell), typeof(CustomShellRenderer))]
namespace PartnerClient.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            TouchEffectPreserver.Preserve();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            Sharpnado.HorizontalListView.iOS.SharpnadoInitializer.Initialize();

            //ThemeEngine.OnThemeChanged += e => App.Window.OverrideUserInterfaceStyle = e.IsEffectivelyLight ? UIUserInterfaceStyle.Light : UIUserInterfaceStyle.Dark;
            if (Window != null)
                //ThemeEngine.OnThemeChanged += e => Window.RootViewController.OverrideUserInterfaceStyle = e.IsEffectivelyLight ? UIUserInterfaceStyle.Light : UIUserInterfaceStyle.Dark;
                ThemeEngine.OnThemeChanged += e =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
                        //GetViewController().SetNeedsStatusBarAppearanceUpdate();
                    });
                };


            Xamarin.Forms.Forms.SetFlags("IndicatorView_Experimental", "SwipeView_Experimental", "AppTheme_Experimental", "Expander_Experimental", "Shapes_Experimental", "Brush_Experimental");
            global::Xamarin.Forms.Forms.Init();
            AiForms.Dialogs.Dialogs.Init();
            Sharpnado.MaterialFrame.iOS.iOSMaterialFrameRenderer.Init();
            UINavigationBar.Appearance.Translucent = false;
            Firebase.Core.App.Configure();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

    }
}
