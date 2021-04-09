using PartnerClient.Models;
using PartnerClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OnboardingPage : ContentPage
    {
        public OnboardingViewModel ViewModel { get; set; } = new OnboardingViewModel();

        public OnboardingPage()
        {
            InitializeComponent();
            SwitchButtons.Switched += SwitchButtons_Switched;
            if (string.IsNullOrEmpty(PartnerUser.Current.TeamUID))
                PendingGrid.Opacity = 0d;
            ViewModel.StateChanged += (isCreating, isJoining) => PendingGrid.FadeTo(isCreating || isJoining ? 1d : 0d);
        }

        private void SwitchButtons_Switched(object sender, bool e)
        {
            var join = (bool)e;
            JoinForm.FadeTo(join ? 1d : 0d);
            CreateForm.FadeTo(join ? 0d : 1d);
        }
    }
}