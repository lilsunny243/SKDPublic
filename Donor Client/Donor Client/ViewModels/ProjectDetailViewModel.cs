using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs;
using DonorClient.Models;
using DonorClient.Views;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using Xamarin.Forms;

namespace DonorClient.ViewModels
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
        public string DatePublishedString { get => datePublishedString; private set => SetProperty(ref datePublishedString, value); }

        public bool UserHasDonated => source?.DonorUIDs?.Contains(DonorUser.Current?.UID ?? string.Empty) ?? false;

        public ProjectDetailViewModel()
        {
            DonorUser.CurrentChanged += _ => OnPropertyChanged(nameof(UserHasDonated));
            DonorUser.CurrentUpdated += OnUserUpdated;
            OnUserUpdated(DonorUser.Current);
        }

        private void OnUserUpdated(DonorUser? user) => GiftAidEnabled = user?.GiftAidEnabled ?? false;

        public SortedObservableCollection<ProjectUpdateCardViewModel, ProjectUpdate, DateTime> UpdateViewModels { get; } = new SortedObservableCollection<ProjectUpdateCardViewModel, ProjectUpdate, DateTime>(x => x.Source!.DatePublished.GetValueOrDefault(), true);

        private IListenerRegistration? docListener;
        private IListenerRegistration? updateListener;
        public void Init(string projectUID)
        {
            docListener?.Remove();
            updateListener?.Remove();
            DonationAmount = -2;
            doc = Firestore
            .Collection("Projects")
            .Document(projectUID);
            docListener = doc.AddSnapshotListener(OnSnapshot);
            DonorUser.CurrentChanged += OnUserChanged;
            OnUserChanged(DonorUser.Current);
        }

        private void OnUserChanged(DonorUser? user)
        {
            updateListener?.Remove();
            if (!(user is null))
            {
                updateListener = FirestoreCollectionService
                    .Subscribe(doc!.Collection("Updates")
                    .WhereEqualsTo(nameof(ProjectUpdate.Status), nameof(ProjectUpdateStatus.Published))
                    .WhereArrayContains(nameof(ProjectUpdate.AllowedDonorUIDs), user.UID),
                    UpdateViewModels, x => new ProjectUpdateCardViewModel(x));
            }
        }

        private void OnSnapshot(IDocumentSnapshot? snapshot, Exception? ex)
        {
            if (!(snapshot?.Exists is true)) return;
            source = snapshot.ToObject<Project>()!;
            (Title, Description) = Culture.IsSpanish
                ? (source.NameEs, source.DescriptionEs)
                : (source.NameEn, source.DescriptionEn);
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
            OnPropertyChanged(nameof(UserHasDonated));
        }


        private int donationAmount;
        public int DonationAmount { get => donationAmount; set => SetProperty(ref donationAmount, value, onlyIf: x => x == -2 || x >= 0); }


        private bool giftAidEnabled;
        public bool GiftAidEnabled { get => giftAidEnabled; set => SetProperty(ref giftAidEnabled, value); }


        public Command<int> AddDonationAmountCommand => new Command<int>(amount
            => DonationAmount += DonationAmount <= 0 ? amount - DonationAmount : amount);


        public Command DonateCommand => new Command(ExecuteDonateCommand);

        private async void ExecuteDonateCommand()
        {
            if (await Dialog.Instance.ShowAsync<DonationAmountSelectionView>(this))
            {
                var donation = new Donation(source!, DonationAmount);
                await DonationBundle.Current!.AddDonationAsync(donation);
                DonationAmount = 0;
            }
        }

    }
}
