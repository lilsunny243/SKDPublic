using System;
using PartnerClient.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartnerClient.Views
{
    [QueryProperty(nameof(ProjectUID), "uid")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProjectCreationPage : ContentPage
    {
        public ProjectCreationViewModel ViewModel { get; } = new ProjectCreationViewModel();

        public string ProjectUID { set => ViewModel.LoadProjectDraft(Uri.UnescapeDataString(value)); }

        public ProjectCreationPage()
        {
            InitializeComponent();
            Shell.Current.Navigated += (s, e) =>
            {
                if (e.Previous?.Location.OriginalString != e.Current.Location.OriginalString 
                && (e.Previous?.Location.OriginalString.EndsWith("ProjectCreation") ?? false))
                    ViewModel.Reset();
            };
        }
    }
}