using System.Collections.Generic;
using System.Linq;
using PartnerClient.Models;
using PartnerClient.Resources;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using Xamarin.Forms;

namespace PartnerClient.ViewModels
{
    public class ProjectsViewModel : PageViewModel
    {
        private bool isBusy;
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value, RefreshCommand.ChangeCanExecute); }

        private string? selectedProjectUID;
        public string? SelectedProjectUID { get => selectedProjectUID; set => SetProperty(ref selectedProjectUID, value); }

        public GroupedObservableCollection<ProjectStatus, ProjectCardViewModel> CardViewModels { get; }

        private Command? _refreshCommand;
        public Command RefreshCommand => _refreshCommand ??= new Command(() => LoadProjects(Team.Current), () => !IsBusy);

        public Command CreateCommand => new Command(ExecuteCreateCommand);
        public Command CardTappedCommand => new Command<Project>(OnCardTapped);

        private readonly List<ImpactMeasurementArea> activeFilterIMAs = new List<ImpactMeasurementArea>();
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

        public ProjectsViewModel()
        {
            Title = AppResources.Projects;
            CardViewModels = new GroupedObservableCollection<ProjectStatus, ProjectCardViewModel>(
                vm => vm.Source!.Status,
                vm => !activeFilterIMAs.Any() || vm.Tags.Any(tag => activeFilterIMAs.Contains(tag.IMA)),
                vm => vm.IsUrgent,
                (ProjectStatus.Draft, AppResources.DraftApplications),
                (ProjectStatus.Pending, AppResources.PendingApplications),
                (ProjectStatus.Active, AppResources.ActiveProjects),
                (ProjectStatus.Completed, AppResources.CompletedProjects));
            LoadProjects(Team.Current);
            Team.CurrentChanged += LoadProjects;
        }

        private IListenerRegistration? projectsListener;
        private void LoadProjects(Team? team)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                projectsListener?.Remove();
                if (!(team is null))
                {
                    var projectsQuery = Firestore.Collection("Projects")
                        .WhereEqualsTo(nameof(Project.PartnerUID), team.UID);
                    projectsListener = FirestoreCollectionService.Subscribe<Project, ProjectCardViewModel, ProjectStatus>(
                        projectsQuery,
                        CardViewModels,
                        x => new ProjectCardViewModel(x),
                        () => IsBusy = false);
                }
                else
                    IsBusy = false;
            }
        }

        private async void ExecuteCreateCommand() => await Shell.Current.GoToAsync("ProjectCreation");

        private async void OnCardTapped(Project project)
        {
            if (project.Status == ProjectStatus.Draft)
                await Shell.Current.GoToAsync($"ProjectCreation?uid={project.UID}");
            else
            {
                SelectedProjectUID = project.UID;
                await Shell.Current.GoToAsync($"ProjectDetails?uid={project.UID}");
            }
        }
    }
}
