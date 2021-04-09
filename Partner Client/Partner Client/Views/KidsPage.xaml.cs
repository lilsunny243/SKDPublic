using PartnerClient.ViewModels;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KidsPage : ContentPage
    {
        public KidsViewModel ViewModel { get; } = new KidsViewModel();

        public KidsPage()
        {
            InitializeComponent();
        }
    }
}