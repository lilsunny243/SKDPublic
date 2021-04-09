using System;
using System.Collections.Generic;
using System.Text;
using DonorClient.Models;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Services;

namespace DonorClient.ViewModels
{
    public class PastDonationBundleViewModel : BaseDonationBundleViewModel
    {
        private bool hadFirstLoad;
        private IListenerRegistration? docListener;
        private GiftAidState giftAidState;
        private bool giftAidUnclaimed;
        private string dateString = string.Empty;
        private string lastFour = string.Empty;

        private bool usedCard;
        private bool usedApplePay;
        private bool usedGooglePay;

        public bool UsedCard { get => usedCard; private set => SetProperty(ref usedCard, value); }
        public bool UsedGooglePay { get => usedGooglePay; private set => SetProperty(ref usedGooglePay, value); }
        public bool UsedApplePay { get => usedApplePay; private set => SetProperty(ref usedApplePay, value); }

        public string DateString { get => dateString; private set => SetProperty(ref dateString, value); }
        public string LastFour { get => lastFour; private set => SetProperty(ref lastFour, value); }

        public GiftAidState GiftAidState { get => giftAidState; private set => SetProperty(ref giftAidState, value); }
        public bool GiftAidUnclaimed { get => giftAidUnclaimed; private set => SetProperty(ref giftAidUnclaimed, value); }

        public bool Confirmed => true;

        public PastDonationBundleViewModel() => Title = "Donation Bundle";

        public void Init(string uid)
        {
            docListener?.Remove();
            hadFirstLoad = false;
            var doc = DonorUser.Current?.Doc.Collection("DonationBundles").Document(uid);
            docListener = doc?.AddSnapshotListener(OnSnapshot);
        }

        protected override void OnBundleChanged(DonationBundle? bundle)
        {
            base.OnBundleChanged(bundle);
        }

        protected override void OnBundleUpdated(DonationBundle? bundle)
        {
            base.OnBundleUpdated(bundle);
            GiftAidState = bundle?.GiftAidState ?? GiftAidState.Ineligible;
            GiftAidEnabled = GiftAidState != GiftAidState.Ineligible && Status is StripeStatus.Succeeded;
            GiftAidUnclaimed = GiftAidState is GiftAidState.Unclaimed && Status is StripeStatus.Succeeded;
            DateString = bundle?.DateProcessed?.ToShortDateString() ?? string.Empty;
            LastFour = bundle?.PaymentMethodLastFour ?? string.Empty;
            UsedCard = bundle?.PaymentMethodType == PaymentMethodType.Card;
            UsedApplePay = bundle?.PaymentMethodType == PaymentMethodType.ApplePay;
            UsedGooglePay = bundle?.PaymentMethodType == PaymentMethodType.GooglePay;
            CoverStripeFee = bundle?.CoverStripeFee ?? false;
        }

        private void OnSnapshot(IDocumentSnapshot? snapshot, Exception? ex)
        {
            var bundle = snapshot?.ToObject<DonationBundle>();
            if (!hadFirstLoad)
            {
                OnBundleChanged(bundle);
                hadFirstLoad = false;
            }
            OnBundleUpdated(bundle);
        }
    }
}
