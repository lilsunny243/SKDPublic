using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using MaterialFrame = Sharpnado.MaterialFrame.MaterialFrame;

namespace SKD.Common.Themes
{
    public static class ThemeEngine
    {
        private static Theme currentTheme = Theme.System;

        public static void Init(Application app) => app.RequestedThemeChanged += (s, e) => OnSystemThemeUpdated();

        public static event Action<ThemeChangedEventArgs> OnThemeChanged;

        private static void OnSystemThemeUpdated()
        {
            if (currentTheme == Theme.System)
                SetTheme(currentTheme);
        }

        public static bool SetTheme(Theme theme, bool invokeChanged = true)
        {
            bool themeChanged = currentTheme != theme;
            if (themeChanged)
            {
                currentTheme = theme;
                Application.Current.UserAppTheme = theme switch
                {
                    Theme.Light => OSAppTheme.Light,
                    Theme.Dark => OSAppTheme.Dark,
                    _ => OSAppTheme.Unspecified
                };
            }
            if (invokeChanged)
                OnThemeChanged?.Invoke(new ThemeChangedEventArgs(IsEffectivelyLight));
            return themeChanged;
        }

        public static void RefreshTheme() => SetTheme(currentTheme);

        public static bool IsEffectivelyLight => currentTheme == Theme.Light || (currentTheme == Theme.System && AppInfo.RequestedTheme == AppTheme.Light);

    }

    public enum Theme
    {
        System,
        Light,
        Dark
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeChangedEventArgs(bool isLight)
        {
            IsEffectivelyLight = isLight;
            ChromeBackgroundColour = (Color)Application.Current.Resources[isLight ? "ChromeBackgroundColourLight" : "ChromeBackgroundColourDark"];
        }

        public bool IsEffectivelyLight { get; private set; }
        public Color ChromeBackgroundColour { get; private set; }

    }

}
