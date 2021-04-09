using DonorClient.Models;
using DonorClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentBundlePage : ContentPage
    {
        public CurrentBundleViewModel ViewModel { get; } = new CurrentBundleViewModel();

        private double envelopeAnimationValue;
        public double EnvelopeAnimationValue
        {
            get => envelopeAnimationValue; private set
            {
                envelopeAnimationValue = value;
                OnPropertyChanged();
            }
        }

        public CurrentBundlePage()
        {
            InitializeComponent();
            DonationBundle.CurrentConfirmed += RunAnimation;
            DonationBundle.CurrentProcessed += async succeeded =>
            {
                ViewModel.AllowStatusUpdate = false;
                await Task.Delay(800);
                _ = StatusSL.ScaleTo(0d, 500, Easing.CubicInOut);
                await Task.Delay(250);
                ViewModel.Confirmed = false;
                await Task.Delay(500);
                ViewModel.AllowStatusUpdate = true;
                ResetAnimation(false);
            };
            ViewModel.ConfirmationCancelled += () => StatusSL.Scale = 1;
            ResetAnimation(false);
        }

        private void ResetAnimation(bool hideStatusSL)
        {
            BundleEnvelope.Scale = 0d;
            BundleEnvelope.TranslationX = 0d;
            EnvelopeAnimationValue = 0d;
            ThankYouLabel.Opacity = 0d;
            StatusSL.Scale = hideStatusSL ? 0d : 1d;
        }

        private async void RunAnimation()
        {
            ResetAnimation(true);
            await Task.Delay(300);
            await BundleEnvelope.ScaleTo(1.2d, 500, Easing.CubicInOut);
            this.Animate("EnvelopeAnimation", t => EnvelopeAnimationValue = t, 0d, 1d, length: 500, easing: Easing.CubicInOut);
            await Task.Delay(500);
            _ = BundleEnvelope.TranslateTo(Width, 0d, 600, Easing.SpringIn);
            await Task.Delay(300);
            await ThankYouLabel.FadeTo(1d, 500, Easing.CubicInOut);
            await Task.Delay(800);
            _ = ThankYouLabel.FadeTo(0d, 500, Easing.CubicInOut);
            await StatusSL.ScaleTo(1d, 500, Easing.CubicInOut);
        }

        private void ConfirmButton_Clicked(object sender, EventArgs e) => StatusSL.Scale = 0;
    }

    public class NativePayButton : View
    {
        public static readonly BindableProperty AppleCommandProperty = BindableProperty.Create(nameof(AppleCommand), typeof(ICommand), typeof(NativePayButton));
        public static readonly BindableProperty GoogleCommandProperty = BindableProperty.Create(nameof(GoogleCommand), typeof(ICommand), typeof(NativePayButton));

        public ICommand? AppleCommand { get => (ICommand?)GetValue(AppleCommandProperty); set => SetValue(AppleCommandProperty, value); }
        public ICommand? GoogleCommand { get => (ICommand?)GetValue(GoogleCommandProperty); set => SetValue(GoogleCommandProperty, value); }

        public event EventHandler? AppleClicked;
        public event EventHandler? GoogleClicked;

        public void SendAppleClicked()
        {
            AppleClicked?.Invoke(this, new EventArgs());
            AppleCommand?.Execute(null);
        }

        public void SendGoogleClicked()
        {
            GoogleClicked?.Invoke(this, new EventArgs());
            GoogleCommand?.Execute(null);
        }
    }
}