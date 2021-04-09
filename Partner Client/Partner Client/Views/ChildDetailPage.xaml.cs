using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnerClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [QueryProperty(nameof(UID), "uid")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChildDetailPage : ContentPage
    {
        public ChildDetailViewModel ViewModel { get; } = new ChildDetailViewModel();

        public string UID { set => ViewModel.Init(Uri.UnescapeDataString(value)); }

        public ChildDetailPage()
        {
            InitializeComponent();
        }
    }
}