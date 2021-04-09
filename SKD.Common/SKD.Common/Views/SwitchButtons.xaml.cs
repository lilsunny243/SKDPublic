using System;
using System.Collections.Generic;
using SKD.Common.Themes;
using SKD.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SKD.Common.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class SwitchButtons<T> : ContentView
    {

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<SwitchButtonViewModel<T>>), typeof(SwitchButtons<T>), propertyChanged: OnItemsSourceChanged);

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var sb = bindable as SwitchButtons<T>;
            var viewModels = newValue as IEnumerable<SwitchButtonViewModel<T>>;
            sb!.sl.Children.Clear();
            foreach (var vm in viewModels!)
            {
                var button = new SwitchButton(vm);
                button.Clicked += sb.Button_Clicked;
                button.BackgroundColor = sb.ButtonBackgroundColor;
                sb.sl.Children.Add(button);
            }
        }

        public IEnumerable<SwitchButtonViewModel<T>> ItemsSource { get => (IEnumerable<SwitchButtonViewModel<T>>)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }


        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(Command<T>), typeof(SwitchButtons<T>));

        public Command<T> Command { get => (Command<T>)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }


        public static readonly BindableProperty ButtonBackgroundColorProperty = BindableProperty.Create(nameof(ButtonBackgroundColor), typeof(Color), typeof(SwitchButtons<T>), propertyChanged: OnButtonBackgroundColourChanged);

        private static void OnButtonBackgroundColourChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var sb = bindable as SwitchButtons<T>;
            var colour = (Color)newValue;
            foreach (SwitchButton button in sb!.sl.Children)
                button.BackgroundColor = colour;
        }

        public Color ButtonBackgroundColor { get => (Color)GetValue(ButtonBackgroundColorProperty); set => SetValue(ButtonBackgroundColorProperty, value); }


        public event EventHandler<T>? Switched;

        private readonly StackLayout sl;
        public SwitchButtons()
        {
            sl = new StackLayout() { Orientation = StackOrientation.Horizontal, Spacing = 1d };
            Content = new Frame()
            {
                CornerRadius = 12f,
                BorderColor = Color.Transparent,
                BackgroundColor = Color.Transparent,
                Padding = 0,
                Content = sl,
                IsClippedToBounds = true
            };
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var button = sender as SwitchButton;
            Switched?.Invoke(this, button!.ViewModel.Param);
            Command?.Execute(button!.ViewModel.Param);
        }

        private class SwitchButton : Button
        {
            public SwitchButtonViewModel<T> ViewModel { get; }

            public SwitchButton(SwitchButtonViewModel<T> vm)
            {
                ViewModel = vm;
                Text = vm.Text;
                CornerRadius = 0;
                HorizontalOptions = LayoutOptions.FillAndExpand;
                Update();
                vm.PropertyChanged += (s, e) => Update();
                ThemeEngine.OnThemeChanged += e => Update();
            }

            private void Update()
            {
                FontFamily = "LemonMilk" + (ViewModel.IsSelected ? "Regular" : "Light");
                string textColourKey = (ViewModel.IsSelected ? "Primary" : "Unselected")
                    + "TextColour" + (ThemeEngine.IsEffectivelyLight ? "Light" : "Dark");
                TextColor = (Color)Application.Current.Resources[textColourKey];
            }
        }
    }
}