using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnerClient.ViewModels;
using Plugin.FirebaseAuth;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(EmailVerified), "emailVerified")]
    public partial class InvalidAuthPage : ContentPage
    {
        public InvalidAuthViewModel ViewModel { get; } = new InvalidAuthViewModel();

        public string EmailVerified { set => ViewModel.EmailVerified = bool.Parse(Uri.UnescapeDataString(value)); }

        public InvalidAuthPage()
        {
            InitializeComponent();
        }
    }
}