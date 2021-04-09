using AiForms.Dialogs.Abstractions;
using DonorClient.Services;
using DonorClient.ViewModels;
using Plugin.FirebaseAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthProviderSelectionView : DialogView
    {
        public AuthProviderSelectionView(IEnumerable<LoginButtonViewModel> buttonViewModels, AuthProvider pendingCredentialProvider)
        {
            InitializeComponent();
            ButtonSL.SetValue(BindableLayout.ItemsSourceProperty, buttonViewModels);
            InformPendingCredentialLinkLabel.Text = $"Your {pendingCredentialProvider} account will be automatically linked to your profile.";
        }

        public override void RunPresentationAnimation()
        {
            base.RunPresentationAnimation();
            int t = 0;
            foreach (Frame f in ButtonSL.Children)
            {
                t += 120;
                f.TranslationY = 50;
                Device.StartTimer(TimeSpan.FromMilliseconds(t), () =>
                {
                    f.TranslateTo(0, 0, 320, Easing.SinOut);
                    f.FadeTo(1, 320, Easing.SinOut);
                    return false;
                });
            }
        }

        private void CloseButton_Clicked(object sender, EventArgs e) => DialogNotifier.Cancel();
    }
}