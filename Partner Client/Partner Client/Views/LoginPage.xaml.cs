using PartnerClient.ViewModels;
using SKD.Common.Themes;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginViewModel ViewModel { get; } = new LoginViewModel();
        public static event Action Animating;
        private bool _signUp = true;
        private bool justSwitched;
        private bool entryFocused;
        private bool passwordStrengthVisible = false;

        public LoginPage()
        {
            InitializeComponent();
            Device.StartTimer(TimeSpan.FromMilliseconds(150), UpdateChevrons);
            BackgrndImage.Finish += OnBackgrndImageLoaded;
            Shell.Current.Navigated += OnShellNavigated;
            SwitchButtons.Switched += SwitchButtons_Switched;
            PasswordEntry.TextChanged += (s, e) =>
            {
                if (_signUp)
                    UpdatePasswordStrengthLabel(e.NewTextValue);
            };
        }

        private void UpdatePasswordStrengthLabel(string text)
        {
            bool show = !(string.IsNullOrEmpty(text) || passwordStrengthVisible);
            bool hide = string.IsNullOrEmpty(text) && passwordStrengthVisible;
            if (show)
            {
                PasswordStrengthLabel.FadeTo(1, easing: Easing.CubicInOut);
                passwordStrengthVisible = true;
            }
            else if (hide)
            {
                PasswordStrengthLabel.FadeTo(0, easing: Easing.CubicInOut);
                passwordStrengthVisible = false;
            }
        }

        private void SwitchButtons_Switched(object sender, bool signUp)
        {
            justSwitched = true;
            _signUp = signUp;
            if (signUp)
            {
                Button.TranslateTo(0, 0, easing: Easing.CubicInOut);
                PasswordConfirmationEntry.IsEnabled = true;
                PasswordConfirmationSL.FadeTo(1, easing: Easing.CubicInOut);
                UpdatePasswordStrengthLabel(PasswordEntry.Text);
            }
            else
            {
                Button.TranslateTo(0, -(PasswordConfirmationSL.Height + 4), easing: Easing.CubicInOut);
                PasswordConfirmationSL.FadeTo(0, easing: Easing.CubicInOut)
                    .ContinueWith(_ => PasswordConfirmationEntry.IsEnabled = false);
                if (passwordStrengthVisible)
                {
                    PasswordStrengthLabel.FadeTo(0, easing: Easing.CubicInOut);
                    passwordStrengthVisible = false;
                }
            }
        }

        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            if (!entryFocused)
            {
                InputSL.TranslateTo(0, -Height / 3.5, easing: Easing.CubicInOut);
                TitleSL.FadeTo(0, easing: Easing.CubicInOut);
                entryFocused = true;
            }
        }

        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            if (entryFocused)
            {
                Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                {

                    if (EmailEntry.IsFocused || PasswordEntry.IsFocused || PasswordConfirmationEntry.IsFocused || justSwitched)
                        return justSwitched = false;
                    InputSL.TranslateTo(0, 0, easing: Easing.CubicInOut);
                    TitleSL.FadeTo(1, easing: Easing.CubicInOut);
                    return entryFocused = false;
                });
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (animationStarted && entryFocused)
            {
                InputSL.TranslateTo(0, 0, easing: Easing.CubicInOut);
                TitleSL.FadeTo(1, easing: Easing.CubicInOut);
                entryFocused = false;
            }
        }

        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // If we have just arrived after sign out and for some reason this page hasn't been destroyed!
            // We must reset the original visual states of the beginning transition items;
            if (e.Current?.Location.OriginalString == "//Login")
            {
                animationStarted = false;
                MaterialFrame.Opacity = 0;
                LogoImage.TranslationY = 0;
                TitleSL.Opacity = 0;
                TitleSL.TranslationY = 200;
                ChevronSL.Opacity = 0;
                EmailEntry.IsEnabled = PasswordEntry.IsEnabled = PasswordConfirmationEntry.IsEnabled = false;
                foreach (VisualElement v in InputSL.Children)
                    v.Opacity = 0;
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
            MaterialFrame.FadeTo(0.99, 160u * (uint)InputSL.Children.Count, Easing.CubicInOut); // Opacity 1.0 causes text above to become blurred?
            LogoImage.TranslateTo(0, Height * -0.35d, 400, Easing.CubicOut);
            TitleSL.FadeTo(1, 600, Easing.CubicInOut);
            TitleSL.TranslateTo(0, 0, 600, Easing.CubicInOut);
            ChevronSL.FadeTo(0, 100, Easing.CubicOut);
            EmailEntry.IsEnabled = PasswordEntry.IsEnabled = PasswordConfirmationEntry.IsEnabled = true;
            int t = 0;
            foreach (VisualElement v in InputSL.Children)
            {
                t += 160;
                v.TranslationX = -v.Width;
                Device.StartTimer(TimeSpan.FromMilliseconds(t), () =>
                {
                    if (!_signUp)
                    {
                        if (v == Button)
                        {
                            v.TranslateTo(0, -(PasswordConfirmationSL.Height + 4), 400, Easing.SinOut);
                            v.FadeTo(1, 400, Easing.SinOut);
                            return false;
                        }
                        else if (v == PasswordConfirmationSL)
                        {
                            v.TranslateTo(0, 0, 400, Easing.SinOut);
                            return false;
                        }
                    }
                    v.TranslateTo(0, 0, 400, Easing.SinOut);
                    v.FadeTo(1, 400, Easing.SinOut);
                    return false;
                });
            }
            ThemeEngine.RefreshTheme(); // Set nav colours to what they should be on android
        }

    }
}