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
    [QueryProperty(nameof(ProjectUID), "projectUid")]
    [QueryProperty(nameof(LoadDraft), "loadDraft")]
    [QueryProperty(nameof(UpdateUID), "uid")]
    [QueryProperty(nameof(ProjectNameEn), "projectNameEn")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProjectUpdateCreationPage : ContentPage
    {
        private string? projectUid;
        private bool? loadDraft;
        private string? uid;

        public ProjectUpdateCreationViewModel ViewModel { get; } = new ProjectUpdateCreationViewModel();

        public string ProjectUID { set { projectUid = Uri.UnescapeDataString(value); TrySetup(); } }
        public string LoadDraft { set { loadDraft = bool.Parse(Uri.UnescapeDataString(value)); TrySetup(); } }
        public string UpdateUID { set { uid = Uri.UnescapeDataString(value); TrySetup(); } }

        public string ProjectNameEn { set => ViewModel.ProjectNameEn = Uri.UnescapeDataString(value); }

        private void TrySetup()
        {
            if (projectUid is null || loadDraft is null || ((bool)loadDraft && uid is null)) return;
            if ((bool)loadDraft) 
                ViewModel.LoadUpdateDraft(uid!, projectUid);
            else
                ViewModel.SetupCreateForProject(projectUid);
            (projectUid, loadDraft, uid) = (null, null, null);
        }

        public ProjectUpdateCreationPage()
        {
            InitializeComponent();
        }
    }
}