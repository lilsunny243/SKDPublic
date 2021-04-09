using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnerClient.Models;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using Xamarin.Forms;

namespace PartnerClient.ViewModels
{
    public class ProjectDetailViewModel : PageViewModel
    {
        private Project? source;
        private IDocumentReference? doc;

        private string? description;
        private Uri[]? imageURIs;
        private List<ProjectDetailTagViewModel>? tags;
        private Color accentColour;
        private bool isUrgent;
        private List<CostBreakdownComponent>? costBreakdownComponents;
        private string? partnerName;

        public string? Description { get => description; private set => SetProperty(ref description, value); }
        public Uri[]? ImageURIs { get => imageURIs; private set => SetProperty(ref imageURIs, value, RaisePropertyChanged(nameof(ImageCount))); }
        public int ImageCount { get => ImageURIs?.Length ?? 0; }
        public List<ProjectDetailTagViewModel>? Tags { get => tags; private set => SetProperty(ref tags, value); }
        public Color AccentColour { get => accentColour; private set => SetProperty(ref accentColour, value); }
        public List<CostBreakdownComponent>? CostBreakdownComponents { get => costBreakdownComponents; set => SetProperty(ref costBreakdownComponents, value); }

        public string? PartnerName { get => partnerName; set => SetProperty(ref partnerName, value); }
        public bool IsUrgent { get => isUrgent; set => SetProperty(ref isUrgent, value); }

        private ProjectStatus status;
        public ProjectStatus Status { get => status; private set => SetProperty(ref status, value); }

        private int raised;
        private int target;
        public int Target { get => target; private set => SetProperty(ref target, value); }
        public int Raised { get => raised; private set => SetProperty(ref raised, value); }
        public double Progress => Math.Min(Raised / (double)Target, 1d);

        private string datePublishedString = string.Empty;
        public string DatePublishedString { get => datePublishedString; private set => SetProperty(ref datePublishedString, value, RaisePropertyChanged(nameof(IsPublished))); }
        public bool IsPublished => Status == ProjectStatus.Active || Status == ProjectStatus.Completed;

        public SortedObservableCollection<ProjectUpdateCardViewModel, ProjectUpdate, DateTime> UpdateViewModels { get; } = new SortedObservableCollection<ProjectUpdateCardViewModel, ProjectUpdate, DateTime>(x => x.Source!.DateCreated, true);
        public bool HasDraftProgressUpdate => UpdateViewModels.Any(x => x.IsDraft);

        private IListenerRegistration? docListener;
        private IListenerRegistration? updateListener;
        public void Init(string projectUID)
        {
            docListener?.Remove();
            updateListener?.Remove();
            doc = Firestore
            .Collection("Projects")
            .Document(projectUID);
            docListener = doc.AddSnapshotListener(OnSnapshot);
            Team.CurrentChanged += OnTeamChanged;
            OnTeamChanged(Team.Current);
        }

        private void OnTeamChanged(Team? team)
        {
            updateListener?.Remove();
            if (!(team is null))
            {
                updateListener = FirestoreCollectionService
                .Subscribe(doc!.Collection("Updates").WhereEqualsTo(nameof(ProjectUpdate.PartnerUID), team.UID),
                UpdateViewModels, x => new ProjectUpdateCardViewModel(x), RaisePropertyChanged(nameof(HasDraftProgressUpdate)));
            }
        }

        private void OnSnapshot(IDocumentSnapshot? snapshot, Exception? ex)
        {
            if (!(snapshot?.Exists is true)) return;
            source = snapshot.ToObject<Project>();
            (Title, Description) = Culture.IsSpanish
                ? (source!.NameEs, source.DescriptionEs)
                : (source!.NameEn, source.DescriptionEn);
            Tags = source.ImpactMeasurementAreas
                .OrderBy(x => x.Key == source.PrimaryImpactMeasurementArea ? int.MinValue : x.Key.GetColourOrder())
                .Select(x => new ProjectDetailTagViewModel(x)).ToList();
            AccentColour = source.PrimaryImpactMeasurementArea.GetAccentColour();
            (Status, Target, Raised, IsUrgent) = (source.Status, source.Target, source.Raised, source.IsUrgent);
            OnPropertyChanged(nameof(Progress));
            ImageURIs = source.Images.Select(x => new Uri(x.Uri)).ToArray();
            CostBreakdownComponents = source.CostBreakdown;
            PartnerName = source.PartnerName;
            DatePublishedString = source.DatePublished?.ToShortDateString() ?? string.Empty;
        }


        public Command CreateProgressUpdateCommand => new Command(ExecuteCreateProgressUpdateCommand);
        private async void ExecuteCreateProgressUpdateCommand()
        {
            if (HasDraftProgressUpdate)
                ExecuteEditProgressUpdateCommand(UpdateViewModels.First(x => x.IsDraft).Source!);
            else
                await Shell.Current.GoToAsync($"ProjectUpdateCreation?projectUid={source!.UID}&projectNameEn={source.NameEn}&loadDraft=false");
        }

        public Command<ProjectUpdate> EditProgressUpdateCommand => new Command<ProjectUpdate>(ExecuteEditProgressUpdateCommand, update => update.Status == ProjectUpdateStatus.Draft);
        private async void ExecuteEditProgressUpdateCommand(ProjectUpdate update)
        {
            await Shell.Current.GoToAsync($"ProjectUpdateCreation?uid={update.UID}&projectUid={source!.UID}&projectNameEn={source.NameEn}&loadDraft=true");
        }


    }
}
