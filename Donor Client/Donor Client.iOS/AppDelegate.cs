using System;
using DonorClient.Views;
using Foundation;
using Google.SignIn;
using Plugin.FirebasePushNotification;
using SKD.Common.iOS;
using SKD.Common.Themes;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationFetcher))]
[assembly: ExportRenderer(typeof(DonorClient.AppShell), typeof(CustomShellRenderer))]
namespace DonorClient.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //

        private bool isCreditCardInputShowing = false;
        UIVisualEffectView blurWindow = null;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            TouchEffect.iOS.TouchEffectPreserver.Preserve();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();

            Stripe.iOS.ApiClient.SharedClient.Configuration.AppleMerchantIdentifier = App.AppleMerchantId;
            Stripe.iOS.ApiClient.SharedClient.PublishableKey = App.StripeKey;

            //ThemeEngine.OnThemeChanged += e => Window.OverrideUserInterfaceStyle = e.IsEffectivelyLight ? UIUserInterfaceStyle.Light : UIUserInterfaceStyle.Dark;
            if (Window != null)
                ThemeEngine.OnThemeChanged += e => Window.RootViewController.OverrideUserInterfaceStyle = e.IsEffectivelyLight ? UIUserInterfaceStyle.Light : UIUserInterfaceStyle.Dark;

            Forms.SetFlags("IndicatorView_Experimental", "AppTheme_Experimental", "Expander_Experimental", "Shapes_Experimental", "Brush_Experimental", "SwipeView_Experimental");
            Forms.Init();
            AiForms.Dialogs.Dialogs.Init();
            Sharpnado.MaterialFrame.iOS.iOSMaterialFrameRenderer.Init();
            AuthenticationService.Init(this);
            ApplePayService.Init(this);
            UINavigationBar.Appearance.Translucent = false;
            CreditCardInputView.Appearing += () => isCreditCardInputShowing = true;
            CreditCardInputView.Disappearing += () => isCreditCardInputShowing = false;

            Firebase.Core.App.Configure();

            LoadApplication(new App());
            FirebasePushNotificationManager.Initialize(options, true);
            return base.FinishedLaunching(app, options);
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);
            blurWindow?.RemoveFromSuperview();
            blurWindow?.Dispose();
            blurWindow = null;
        }

        public override void OnResignActivation(UIApplication uiApplication)
        {
            base.OnResignActivation(uiApplication);
            if (isCreditCardInputShowing)
            {
                using var blurEffect = UIBlurEffect.FromStyle(ThemeEngine.IsEffectivelyLight ? UIBlurEffectStyle.ExtraLight : UIBlurEffectStyle.Dark);
                blurWindow = new UIVisualEffectView(blurEffect)
                {
                    Frame = UIApplication.SharedApplication.KeyWindow.RootViewController.View.Bounds
                };
                UIApplication.SharedApplication.KeyWindow.RootViewController.View.AddSubview(blurWindow);
            }
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            FirebasePushNotificationManager.DidRegisterRemoteNotifications(deviceToken);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            FirebasePushNotificationManager.RemoteNotificationRegistrationFailed(error);

        }
        // To receive notifications in foregroung on iOS 9 and below.
        // To receive notifications in background in any iOS version
        //public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        //{
        //    // If you are receiving a notification message while your app is in the background,
        //    // this callback will not be fired 'till the user taps on the notification launching the application.

        //    // If you disable method swizzling, you'll need to call this method. 
        //    // This lets FCM track message delivery and analytics, which is performed
        //    // automatically with method swizzling enabled.
        //    //FirebasePushNotificationManager.DidReceiveMessage(userInfo);
        //    // Do your magic to handle the notification data
        //    //Console.WriteLine(userInfo);

        //    completionHandler(UIBackgroundFetchResult.NewData);
        //}

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (SignIn.SharedInstance.HandleUrl(url))
                return true;
            if (Xamarin.Essentials.Platform.OpenUrl(app, url, options))
                return true;

            return base.OpenUrl(app, url, options);
        }
    }
}
