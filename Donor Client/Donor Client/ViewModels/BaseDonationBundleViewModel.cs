using DonorClient.Models;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DonorClient.ViewModels
{
    public abstract class BaseDonationBundleViewModel : PageViewModel
    {

        private StripeStatus status;
        private int amount;
        private bool giftAidEnabled;
        protected bool allowStatusUpdate = true;
        private bool coverStripeFee;

        public StripeStatus Status { get => status; protected set => SetProperty(ref status, value); }
        public int Amount { get => amount; protected set => SetProperty(ref amount, value, RaisePropertyChanged(nameof(StripeFee), nameof(AmountAfterFee), nameof(TotalReceived), nameof(TotalPayed))); }
        public bool GiftAidEnabled { get => giftAidEnabled; protected set => SetProperty(ref giftAidEnabled, value, RaisePropertyChanged(nameof(TotalReceived))); }


        public bool CoverStripeFee { get => coverStripeFee; set => SetProperty(ref coverStripeFee, value, RaisePropertyChanged(nameof(StripeFee), nameof(AmountAfterFee), nameof(TotalReceived), nameof(TotalPayed))); }
        public int StripeFee => (int)Math.Ceiling(((Amount * App.StripeFeeCoefficient) + App.StripeFeeConstant) * (CoverStripeFee ? (1 / (1 - App.StripeFeeCoefficient)) : 1));
        public int AmountAfterFee => CoverStripeFee ? Amount : Amount - StripeFee;
        public int TotalReceived => GiftAidEnabled ? (int)Math.Round(AmountAfterFee * 1.25d) : AmountAfterFee;
        public int TotalPayed => CoverStripeFee ? Amount + StripeFee : Amount;

        public SortedObservableCollection<DonationViewModel, Donation, int> DonationViewModels { get; } = new SortedObservableCollection<DonationViewModel, Donation, int>(x => x.Source!.Amount);
        
        private IListenerRegistration? donationListener;
        protected virtual void OnBundleChanged(DonationBundle? bundle)
        {
            donationListener?.Remove();
            if (bundle is null) return;
            donationListener = FirestoreCollectionService
                .Subscribe(bundle.Doc.Collection("Donations"),
                DonationViewModels, x => new DonationViewModel(x));
        }

        protected virtual void OnBundleUpdated(DonationBundle? bundle)
        {
            if (!(bundle is null) && allowStatusUpdate)
            {
                Amount = bundle.Amount;
                Status = bundle.StripeStatus;
            }
        }
    }

    public class DonationViewModel : IndexedCardViewModel<Donation>
    {
        private int amount;
        private string projectName;

        public DonationViewModel(Donation donation) : base(donation)
        {
            projectName = donation.ProjectNameEn;
            amount = donation.Amount;
            Tags = donation.ProjectTags.Select(x => new CardTagViewModel(x)).ToList();
        }
        
        public override void Update(Donation donation)
        {
            base.Update(donation);
            (ProjectName, Amount) = (donation.ProjectNameEn, donation.Amount);
        }

        public string ProjectName { get => projectName; set => SetProperty(ref projectName, value); }
        public int Amount { get => amount; set => SetProperty(ref amount, value); }

        public List<CardTagViewModel>? Tags { get; private set; }
    }
}
