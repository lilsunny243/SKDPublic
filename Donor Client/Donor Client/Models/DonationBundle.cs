using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using Plugin.CloudFirestore.Converters;
using SKD.Common.Models;
using SKD.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DonorClient.Models
{
    public class DonationBundle : BaseModel
    {
        [Ignored]
        public IDocumentReference Doc => DonorUser.Current!.Doc.Collection("DonationBundles").Document(UID);

        public int Amount { get; set; }
        public string? PaymentMethodUID { get; set; }
        public string? PaymentMethodLastFour { get; set; }
        public string? PaymentMethodOneTimeToken { get; set; }

        [DocumentConverter(typeof(EnumStringConverter))]
        public PaymentMethodType PaymentMethodType { get; set; }

        public bool UserConfirmed { get; set; }
        public bool CoverStripeFee { get; set; }
        public DateTime? DateProcessed { get; set; }

        [DocumentConverter(typeof(EnumStringConverter))]
        public GiftAidState GiftAidState { get; set; }

        public string? StripeId { get; set; }

        [DocumentConverter(typeof(StripeStatusConverter))]
        public StripeStatus StripeStatus { get; set; } = StripeStatus.Pending;

        public string? StripeRedirectUrl { get; set; }

        [Ignored]
        public static DonationBundle? Current { get; private set; }

        public static event Action<DonationBundle?>? CurrentChanged;
        public static event Action<DonationBundle?>? CurrentUpdated;
        public static event Action? CurrentConfirmed;
        public static event Action<bool>? CurrentProcessed;

        private static IListenerRegistration? listener;
        public static async Task SetCurrentAsync(IDocumentReference? doc)
        {
            if (doc?.Id != Current?.Doc.Id || doc is null)
            {
                listener?.Remove();

                if (doc is null)
                {
                    if (DonorUser.Current is null)
                    {
                        Current = null;
                        previousUID = null;
                        CurrentUpdated?.Invoke(null);
                        CurrentChanged?.Invoke(null);
                        return;
                    }
                    else
                    {
                        var bundleCollection = DonorUser.Current.Doc.Collection("DonationBundles");
                        DonationBundle bundle = new DonationBundle
                        {
                            StripeStatus = StripeStatus.Pending,
                            GiftAidState = DonorUser.Current.GiftAidEnabled ? GiftAidState.Unclaimed : GiftAidState.Ineligible,
                            Amount = 0,
                            UserConfirmed = false
                        };
                        doc = await bundleCollection.AddAsync(bundle);
                        await DonorUser.Current.Doc.UpdateAsync(new { CurrentDonationBundle = doc });
                    }
                }
                listener = doc.AddSnapshotListener((snapshot, ex) => OnCurrentSnapshot(snapshot));
            }
        }

        private static string? previousUID;
        private static void OnCurrentSnapshot(IDocumentSnapshot? snapshot)
        {
            Current = snapshot?.ToObject<DonationBundle>();
            CurrentUpdated?.Invoke(Current);
            if (previousUID != Current?.UID)
            {
                previousUID = Current?.UID;
                CurrentChanged?.Invoke(Current);
            }
            if (Current?.StripeStatus is StripeStatus.Succeeded || Current?.StripeStatus is StripeStatus.Failed)
            {
                _ = SetCurrentAsync(null);
                CurrentProcessed?.Invoke(Current?.StripeStatus is StripeStatus.Succeeded);
            }
        }

        public async Task AddDonationAsync(Donation donation)
        {
            await Doc.Collection("Donations").AddAsync(donation);
            Amount += donation.Amount;
            await Doc.UpdateAsync(new { Amount });
        }

        public async Task RemoveDonationAsync(Donation donation)
        {
            await Doc.Collection("Donations").Document(donation.UID).DeleteAsync();
            Amount -= donation.Amount;
            await Doc.UpdateAsync(new { Amount });
        }

        public static async Task ConfirmCurrentWithCardAsync(PaymentMethod paymentMethod, bool coverStripeFee)
        {
            if (!(Current is null))
            {
                await Current.Doc.UpdateAsync(new
                {
                    UserConfirmed = true,
                    PaymentMethodUID = paymentMethod.UID,
                    PaymentMethodLastFour = paymentMethod.LastFourDigits,
                    PaymentMethodType = nameof(PaymentMethodType.Card),
                    CoverStripeFee = coverStripeFee
                });
                CurrentConfirmed?.Invoke();
            }
        }

        public static async Task ConfirmCurrentWithOneTimeTokenAsync(string token, PaymentMethodType paymentMethodType, bool coverStripeFee)
        {
            if (paymentMethodType == PaymentMethodType.Card)
                throw new ArgumentException("Must be Apple or Google Pay", nameof(paymentMethodType));
            if (!(Current is null))
            {
                await Current.Doc.UpdateAsync(new
                {
                    UserConfirmed = true,
                    PaymentMethodType = paymentMethodType.ToString(),
                    PaymentMethodOneTimeToken = token,
                    CoverStripeFee = coverStripeFee
                });
                CurrentConfirmed?.Invoke();
            }
        }
    }

    public enum GiftAidState
    {
        Ineligible,
        Unclaimed,
        Pending,
        Claimed
    }

    public class Donation : BaseModel
    {
        public Donation() { }
        public Donation(Project p, int amount)
            => (ProjectUID, ProjectNameEn, ProjectTags, Amount) = (p.UID, p.NameEn,
            p.ImpactMeasurementAreas.Keys.OrderBy(x => x == p.PrimaryImpactMeasurementArea
            ? int.MinValue
            : x.GetColourOrder()).ToList(), amount);

        public string ProjectUID { get; set; } = string.Empty;
        public string ProjectNameEn { get; set; } = string.Empty;

        [DocumentConverter(typeof(EnumListConverter<ImpactMeasurementArea>))]
        public List<ImpactMeasurementArea> ProjectTags { get; set; } = new List<ImpactMeasurementArea>();

        public int Amount { get; set; }
    }

    public enum PaymentMethodType
    {
        Card,
        ApplePay,
        GooglePay
    }
}
