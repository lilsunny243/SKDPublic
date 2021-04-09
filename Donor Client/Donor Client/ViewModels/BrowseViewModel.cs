using System;
using System.Collections.Generic;
using System.Linq;
using DonorClient.Models;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using SKD.Common.Views;
using Xamarin.Forms;

namespace DonorClient.ViewModels
{
    public class BrowseViewModel : PageViewModel
    {
        private string? selectedProjectUid;
        private bool isBusy;
        private readonly List<ImpactMeasurementArea> activeFilterIMAs = new List<ImpactMeasurementArea>();

        public string? SelectedProjectUID { get => selectedProjectUid; set => SetProperty(ref selectedProjectUid, value); }
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value, RefreshCommand.ChangeCanExecute); }

        private Command? _refreshCommand;
        public Command RefreshCommand => _refreshCommand ??= new Command(LoadProjects, () => !IsBusy);
        public Command<Project> CardTappedCommand => new Command<Project>(OnCardTapped);

        public GroupedObservableCollection<ProjectStatus, ProjectCardViewModel> CardViewModels { get; }

        public Command<IMAFilterItem> FilterCommand => new Command<IMAFilterItem>(ExecuteFilterCommand);
        private void ExecuteFilterCommand(IMAFilterItem item)
        {
            item.IsActive = !item.IsActive;
            if (item.IsActive)
                activeFilterIMAs.Add(item.IMA);
            else
                activeFilterIMAs.Remove(item.IMA);
            CardViewModels.RefreshFilter();
        }

        public List<IMAFilterItem> FilterItems { get; } = new List<IMAFilterItem>()
        {
            new IMAFilterItem(ImpactMeasurementArea.ChildHealth),
            new IMAFilterItem(ImpactMeasurementArea.CommunityRelations),
            new IMAFilterItem(ImpactMeasurementArea.ResponsibleLimits),
            new IMAFilterItem(ImpactMeasurementArea.Education),
            new IMAFilterItem(ImpactMeasurementArea.FamilyRelationships),
            new IMAFilterItem(ImpactMeasurementArea.PositiveValues),
            new IMAFilterItem(ImpactMeasurementArea.SocialIdentity),
            new IMAFilterItem(ImpactMeasurementArea.ConstructiveUseOfTime),
            new IMAFilterItem(ImpactMeasurementArea.SelfEsteemAndDreams),
            new IMAFilterItem(ImpactMeasurementArea.ChristianFaith),
        };

        public BrowseViewModel()
        {
            Title = "Browse";
            CardViewModels = new GroupedObservableCollection<ProjectStatus, ProjectCardViewModel>(
                vm => vm.Source!.Status,
                vm => !activeFilterIMAs.Any() || vm.Tags.Any(tag => activeFilterIMAs.Contains(tag.IMA)),
                x => x.IsUrgent, 
                (ProjectStatus.Active, "Active Projects"),
                (ProjectStatus.Completed, "Completed Projects"));
            DonorUser.CurrentChanged += user => ((user is null) ? null : new Action(LoadProjects))?.Invoke();
            LoadProjects();
        }

        private IListenerRegistration? projectsListener;
        private void LoadProjects()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                projectsListener?.Remove();
                var projectsQuery = Firestore
                    .Collection("Projects")
                    .WhereIn(nameof(Project.Status), new[] { nameof(ProjectStatus.Active), nameof(ProjectStatus.Completed) });
                projectsListener = FirestoreCollectionService.Subscribe<Project, ProjectCardViewModel, ProjectStatus>(
                    projectsQuery,
                    CardViewModels,
                    x => new ProjectCardViewModel(x),
                    () => IsBusy = false);
            }
        }

        private async void OnCardTapped(Project project)
        {
            CardImageOverlayView.Instance?.GetRid();
            SelectedProjectUID = project.UID;
            await Shell.Current.GoToAsync($"ProjectDetails?uid={project.UID}");
        }

    }
}
