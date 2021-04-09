using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonorClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [QueryProperty(nameof(UID), "uid")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PastDonationBundlePage : ContentPage
    {
        public PastDonationBundleViewModel ViewModel { get; } = new PastDonationBundleViewModel();

        public string UID { set => ViewModel.Init(Uri.UnescapeDataString(value)); }

        public PastDonationBundlePage()
        {
            InitializeComponent();
        }
    }
}