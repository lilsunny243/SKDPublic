using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DonorClient.ViewModels;
using SKD.Common.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DonorClient.Views
{
    [QueryProperty(nameof(ProjectUID), "uid")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProjectDetailPage : ContentPage
    {
        public ProjectDetailViewModel ViewModel { get; } = new ProjectDetailViewModel();

        public string ProjectUID { set => ViewModel.Init(Uri.UnescapeDataString(value)); }

        public ProjectDetailPage()
        {
            InitializeComponent();
        }
    }
}