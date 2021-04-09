using DonorClient.Models;
using DonorClient.Services;
using DonorClient.Views;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace DonorClient.ViewModels
{
    public class ProfileViewModel : PageViewModel
    {
        private string? selectedBundleUID;
        public string? SelectedBundleUID { get => selectedBundleUID; private set => SetProperty(ref selectedBundleUID, value); }


        private int totalDonated;
        public int TotalDonated { get => totalDonated; set => SetProperty(ref totalDonated, value); }

        private int giftAidUnclaimed;
        public int GiftAidUnclaimed { get => giftAidUnclaimed; set => SetProperty(ref giftAidUnclaimed, value); }

        private int giftAidClaimed;
        public int GiftAidClaimed { get => giftAidClaimed; set => SetProperty(ref giftAidClaimed, value); }

        public int TotalGiftAid => GiftAidUnclaimed + GiftAidClaimed;
        public int TotalContribution => TotalDonated + TotalGiftAid;

        public ProfileViewModel()
        {
            stripeStati = new ReadOnlyCollection<string>((new StripeStatus[] { StripeStatus.Succeeded, StripeStatus.Failed }).Select(x => x.GetStringValue()!).ToList());
            DonorUser.CurrentUpdated += OnUserUpdated;
            DonorUser.CurrentChanged += OnUserChanged;
            OnUserChanged(DonorUser.Current);
            OnUserUpdated(DonorUser.Current);
        }

        public SortedObservableCollection<ProjectUpdateCardViewModel, ProjectUpdate, DateTime> UpdateViewModels { get; } = new SortedObservableCollection<ProjectUpdateCardViewModel, ProjectUpdate, DateTime>(x => x.Source!.DatePublished.GetValueOrDefault(), true);
        public SortedObservableCollection<DonationBundleCardViewModel, DonationBundle, DateTime> BundleViewModels { get; } = new SortedObservableCollection<DonationBundleCardViewModel, DonationBundle, DateTime>(x => x.Source!.DateProcessed.GetValueOrDefault(), true);

        private readonly ReadOnlyCollection<string> stripeStati;
        private IListenerRegistration? donationBundleListener;
        private IListenerRegistration? projectUpdateListener;
        private void OnUserChanged(DonorUser? user)
        {
            projectUpdateListener?.Remove();
            donationBundleListener?.Remove();
            if (user is null) return;
            projectUpdateListener = FirestoreCollectionService
                .Subscribe(Firestore.CollectionGroup("Updates")
                .WhereArrayContains(nameof(ProjectUpdate.AllowedDonorUIDs), user.UID)
                .WhereEqualsTo(nameof(ProjectUpdate.Status), nameof(ProjectUpdateStatus.Published)),
                UpdateViewModels, x => new ProjectUpdateCardViewModel(x));
            donationBundleListener = FirestoreCollectionService
                .Subscribe(user.Doc.Collection("DonationBundles")
                .WhereIn(nameof(DonationBundle.StripeStatus), stripeStati),
                BundleViewModels, x => new DonationBundleCardViewModel(x));
        }

        private void OnUserUpdated(DonorUser? user)
        {
            Title = user?.Name ?? string.Empty;
            TotalDonated = user?.TotalDonated ?? 0;
            GiftAidUnclaimed = user?.TotalUnclaimedGiftAid ?? 0;
            GiftAidClaimed = user?.TotalClaimedGiftAid ?? 0;
            OnPropertyChanged(nameof(TotalGiftAid));
            OnPropertyChanged(nameof(TotalContribution));
        }

        public Command SettingsCommand => new Command(ExecuteSettingsCommand);
        private async void ExecuteSettingsCommand() => await Shell.Current.GoToAsync("Settings");


        public Command<string> BundleCardTappedCommand => new Command<string>(ExecuteBundleCardTappedCommand);
        private async void ExecuteBundleCardTappedCommand(string uid)
        {
            SelectedBundleUID = uid;
            await Shell.Current.GoToAsync($"PastBundle?uid={uid}");
        }

    }

    public class DonationBundleCardViewModel : IndexedCardViewModel<DonationBundle>
    {
        private int amount;
        private StripeStatus status;
        private GiftAidState giftAidState;
        private int giftAidAmount;
        private string dateString;
        private string lastFour;
        private bool usedCard;
        private bool usedApplePay;
        private bool usedGooglePay;

        private int totalPayed;

        public int Amount { get => amount; private set => SetProperty(ref amount, value); }
        public int GiftAidAmount { get => giftAidAmount; private set => SetProperty(ref giftAidAmount, value); }
        public int TotalPayed { get => totalPayed; private set => SetProperty(ref totalPayed, value); }
        public StripeStatus Status { get => status; private set => SetProperty(ref status, value, RaisePropertyChanged(nameof(IsProcessed))); }
        public GiftAidState GiftAidState
        {
            get => giftAidState; private set => SetProperty(ref giftAidState, value, RaisePropertyChanged(nameof(GiftAidEnabled), nameof(GiftAidClaimed)));
        }
        public bool GiftAidEnabled => GiftAidState != GiftAidState.Ineligible;
        public bool GiftAidClaimed => GiftAidState is GiftAidState.Claimed;
        public string DateString { get => dateString; private set => SetProperty(ref dateString, value); }
        public string LastFour { get => lastFour; private set => SetProperty(ref lastFour, value); }
        public bool UsedCard { get => usedCard; private set => SetProperty(ref usedCard, value); }
        public bool UsedApplePay { get => usedApplePay; private set => SetProperty(ref usedApplePay, value); }
        public bool UsedGooglePay { get => usedGooglePay; private set => SetProperty(ref usedGooglePay, value); }
        public bool IsProcessed => Status is StripeStatus.Succeeded || Status is StripeStatus.Failed;

        public DonationBundleCardViewModel(DonationBundle bundle) : base(bundle)
        {
            amount = bundle.Amount;
            status = bundle.StripeStatus;
            giftAidState = bundle.GiftAidState;
            dateString = bundle.DateProcessed?.ToShortDateString() ?? string.Empty;
            lastFour = bundle.PaymentMethodLastFour ?? string.Empty;
            usedCard = bundle.PaymentMethodType == PaymentMethodType.Card;
            usedApplePay = bundle.PaymentMethodType == PaymentMethodType.ApplePay;
            usedGooglePay = bundle.PaymentMethodType == PaymentMethodType.GooglePay;
            var stripeFee = (int)Math.Ceiling(((Amount * App.StripeFeeCoefficient) + App.StripeFeeConstant) * (bundle.CoverStripeFee ? (1 / (1 - App.StripeFeeCoefficient)) : 1));
            var amountAfterFee = bundle.CoverStripeFee ? Amount : Amount - stripeFee;
            totalPayed = bundle.CoverStripeFee ? Amount + stripeFee : Amount;
            giftAidAmount = (int)(amountAfterFee / 4d);
        }

        public override void Update(DonationBundle bundle)
        {
            base.Update(bundle);
            Amount = bundle.Amount;
            Status = bundle.StripeStatus;
            GiftAidState = bundle.GiftAidState;
            DateString = bundle.DateProcessed?.ToShortDateString() ?? string.Empty;
            LastFour = bundle.PaymentMethodLastFour ?? string.Empty;
            UsedCard = bundle.PaymentMethodType == PaymentMethodType.Card;
            UsedApplePay = bundle.PaymentMethodType == PaymentMethodType.ApplePay;
            UsedGooglePay = bundle.PaymentMethodType == PaymentMethodType.GooglePay;
            var stripeFee = (int)Math.Ceiling(((Amount * App.StripeFeeCoefficient) + App.StripeFeeConstant) * (bundle.CoverStripeFee ? (1 / (1 - App.StripeFeeCoefficient)) : 1));
            var amountAfterFee = bundle.CoverStripeFee ? Amount : Amount - stripeFee;
            TotalPayed = bundle.CoverStripeFee ? Amount + stripeFee : Amount;
            GiftAidAmount = (int)(amountAfterFee / 4d);
        }
    }
}
