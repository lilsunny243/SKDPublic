using DonorClient.Services;
using Plugin.FirebaseAuth;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InvalidAuthPage : ContentPage
    {
        public InvalidAuthPage()
        {
            InitializeComponent();
        }

        private void LogoutButton_Clicked(object sender, System.EventArgs e)
        {
            DependencyService.Get<IAuthenticationService>().SignOut();
            CrossFirebaseAuth.Current.Instance.SignOut();
        }
    }
}