using System;
using System.Threading.Tasks;
using AiForms.Dialogs.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DonationAmountSelectionView : DialogView
    {
        public DonationAmountSelectionView()
        {
            InitializeComponent();
        }

        public override void RunPresentationAnimation()
        {
            MainSL.Opacity = 1;
            MainSL.InputTransparent = false;
            ThankSL.Scale = 0;
            ThankSL.Opacity = 0;
            base.RunPresentationAnimation();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("CloseButton") is Button b)
                b.Clicked += (_, __) => DialogNotifier.Cancel();
        }

        private async void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            MainSL.InputTransparent = true;
            await Task.WhenAll(new[]
            {
                MainSL.FadeTo(0, easing: Easing.CubicInOut),
                ThankSL.ScaleTo(1, 500, Easing.SpringOut),
                ThankSL.FadeTo(1, easing: Easing.CubicInOut),
            });
            await Task.Delay(800);
            DialogNotifier.Complete();
        }
    }
}