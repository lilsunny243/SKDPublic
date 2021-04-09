using DonorClient.ViewModels;
using Plugin.FirebaseAuth;
using SKD.Common.Themes;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginViewModel ViewModel { get; } = new LoginViewModel();
        public static event Action Animating;

        public LoginPage()
        {
            InitializeComponent();
            Device.StartTimer(TimeSpan.FromMilliseconds(150), UpdateChevrons);
            BackgrndImage.Finish += OnBackgrndImageLoaded;
            Shell.Current.Navigated += OnShellNavigated;
        }

        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // If we have just arrived after sign out and for some reason this page hasn't been destroyed!
            // We must reset the original visual states of the beginning transition items;
            if(e.Current?.Location.OriginalString == "//Login" && e.Previous?.Location.OriginalString == "//Settings")
            {
                animationStarted = false;
                MaterialFrame.Opacity = 0;
                LogoImage.TranslationY = 0;
                TitleSL.Opacity = 0;
                TitleSL.TranslationY = 200;
                ChevronSL.Opacity = 0;
                foreach (Frame f in ButtonSL.Children)
                    f.Opacity = 0;
                if (!imageLoaded)
                    BackgrndImage.ReloadImage();
            }
        }

        private bool imageLoaded = false;
        private void OnBackgrndImageLoaded(object sender, FFImageLoading.Forms.CachedImageEvents.FinishEventArgs e)
        {
            if (Shell.Current.CurrentState.Location.OriginalString != "//Login")
                return;
            imageLoaded = true;
            ChevronSL.FadeTo(1, 500, Easing.CubicOut);
            BackgrndImage.FadeTo(1, 500, Easing.CubicOut);
            LogoImage.TranslateTo(0, 80, 750, Easing.CubicInOut);
        }

        private int chevronIndex = 0;
        private bool UpdateChevrons()
        {
            if (chevronIndex < ChevronSL.Children.Count)
                ((Image)ChevronSL.Children[chevronIndex]).FadeTo(0.6, 120, Easing.CubicInOut);
            chevronIndex--;
            if (chevronIndex < 0)
                chevronIndex = ChevronSL.Children.Count;
            else
                ((Image)ChevronSL.Children[chevronIndex]).FadeTo(1, 120, Easing.CubicInOut);
            return true;
        }

        private bool animationStarted = false;
        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (animationStarted)
                return;

            if (LogoImage.TranslationY > -50 && LogoImage.TranslationY <= 0)
            {
                LogoImage.TranslationY += e.TotalY;
                if (LogoImage.TranslationY > 0)
                    LogoImage.TranslationY = 0;
                return;
            }

            animationStarted = true;
            Animating?.Invoke();
            MaterialFrame.FadeTo(0.99, 160u * (uint)ButtonSL.Children.Count, Easing.CubicInOut); // Opacity 1.0 causes text above to become blurred?
            LogoImage.TranslateTo(0, Height * -0.35d, 400, Easing.CubicOut);
            TitleSL.FadeTo(1, 600, Easing.CubicInOut);
            TitleSL.TranslateTo(0, 0, 600, Easing.CubicInOut);
            ChevronSL.FadeTo(0, 100, Easing.CubicOut);
            int t = 0;
            foreach (Frame f in ButtonSL.Children)
            {
                t += 160;
                f.TranslationX = -f.Width;
                Device.StartTimer(TimeSpan.FromMilliseconds(t), () =>
                {
                    f.TranslateTo(0, 0, 400, Easing.SinOut);
                    f.FadeTo(1, 400, Easing.SinOut);
                    return false;
                });
            }
            ThemeEngine.RefreshTheme(); // Set nav colours to what they should be on android
        }
    }
}