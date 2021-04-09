using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SKD.Common.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IconSwapToggleButton : ContentView
    {
        public static readonly BindableProperty IsToggledProperty =
            BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(IconSwapToggleButton), propertyChanged: IsToggledChanged, defaultBindingMode: BindingMode.TwoWay);


        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(IconSwapToggleButton), Color.Default);
        public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }


        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(IconSwapToggleButton), 22d);
        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }


        public static readonly BindableProperty AllowUntoggleProperty = BindableProperty.Create(nameof(AllowUntoggle), typeof(bool), typeof(IconSwapToggleButton), true);
        public bool AllowUntoggle { get => (bool)GetValue(AllowUntoggleProperty); set => SetValue(AllowUntoggleProperty, value); }


        private static void IsToggledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if ((bool)oldValue == (bool)newValue) return;
            var button = bindable as IconSwapToggleButton;
            button.RunAnimation();
        }

        public bool IsToggled { get => (bool)GetValue(IsToggledProperty); set => SetValue(IsToggledProperty, value); }

        public event EventHandler<ToggledEventArgs> Toggled;


        public static readonly BindableProperty TrueIconProperty =
            BindableProperty.Create(nameof(TrueIcon), typeof(string), typeof(IconSwapToggleButton), propertyChanged: OnIconPropertyChanged);

        public static readonly BindableProperty FalseIconProperty =
            BindableProperty.Create(nameof(FalseIcon), typeof(string), typeof(IconSwapToggleButton), propertyChanged: OnIconPropertyChanged);

        private static void OnIconPropertyChanged(BindableObject bindable, object oldValue, object newValue)
            => (bindable as IconSwapToggleButton).RefreshText();

        public string TrueIcon { get => (string)GetValue(TrueIconProperty); set => SetValue(TrueIconProperty, value); }
        public string FalseIcon { get => (string)GetValue(FalseIconProperty); set => SetValue(FalseIconProperty, value); }

        public IconSwapToggleButton()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (IsToggled && !AllowUntoggle) return;
            IsToggled = !IsToggled;
            Toggled?.Invoke(this, new ToggledEventArgs(IsToggled));
        }

        private void RunAnimation()
        {
            Button.RotateTo(360, 500, Easing.CubicInOut).ContinueWith(task => Button.Rotation = 0);
            Button.ScaleTo(0, 250, Easing.CubicIn);
            Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
            {
                RefreshText();
                Button.ScaleTo(1, 250, Easing.CubicOut);
                return false;
            });
        }

        private void RefreshText() => Button.Text = IsToggled ? TrueIcon : FalseIcon;

    }
}