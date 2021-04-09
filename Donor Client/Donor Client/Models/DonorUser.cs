using System;
using System.Collections.Generic;
using System.Linq;
using DonorClient.Models;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using Plugin.FirebasePushNotification;
using SKD.Common.Models;
using SKD.Common.Themes;
using SecureStorage = Xamarin.Essentials.SecureStorage;

namespace DonorClient.Models
{
    public class DonorUser : BaseUser
    {
        public int TotalDonated { get; set; }
        public int TotalClaimedGiftAid { get; set; }
        public int TotalUnclaimedGiftAid { get; set; }
        public List<string> DonatedProjectUIDs { get; set; } = new List<string>();
        public IDocumentReference? CurrentDonationBundle { get; set; }
        public string StripeId { get; set; } = string.Empty;
        public bool GiftAidEnabled { get; set; }
        public GiftAidUserDetails? GiftAidDetails { get; set; }

        public static event Action<DonorUser?>? CurrentUpdated;
        public static event Action<DonorUser?>? CurrentChanged;

        private static IListenerRegistration? listener;
        public static void SetCurrent(string? uid)
        {
            if (uid != Current?.UID)
            {
                listener?.Remove();
                if (string.IsNullOrEmpty(uid))
                {
                    Current = null;
                    previousUID = null;
                    CurrentUpdated?.Invoke(null);
                    CurrentChanged?.Invoke(null);
                    return;
                }
                var doc = Firestore.Collection("Users").Document(uid!);
                listener = doc.AddSnapshotListener((snapshot, ex) => OnCurrentSnapshot(snapshot));
            }
        }

        private static string? previousUID;
        private static void OnCurrentSnapshot(IDocumentSnapshot? snapshot)
        {
            Current = snapshot?.ToObject<DonorUser>();
            CurrentUpdated?.Invoke(Current);
            if (previousUID != Current?.UID)
            {
                previousUID = Current?.UID;
                CurrentChanged?.Invoke(Current);
                CrossFirebasePushNotification.Current.UnsubscribeAll();
                if (!(Current is null))
                    CrossFirebasePushNotification.Current.Subscribe("Urgent_Project");
            }
            _ = DonationBundle.SetCurrentAsync(Current?.CurrentDonationBundle);
            if (!(Current is null) && ThemeEngine.SetTheme(Current.DesiredTheme))
                SecureStorage.SetAsync("ThemePreference", Current.DesiredTheme.ToString());
            var newTopics = Current?.DonatedProjectUIDs.Select(x => "Project_" + x)
                .Where(x => !CrossFirebasePushNotification.Current.SubscribedTopics.Contains(x));
            if (newTopics?.Any() ?? false)
                CrossFirebasePushNotification.Current.Subscribe(newTopics.ToArray());
            System.Diagnostics.Debug.WriteLine(string.Join(", ", CrossFirebasePushNotification.Current.SubscribedTopics));
        }

        [Ignored]
        public static DonorUser? Current { get; private set; }
    }

    public class GiftAidUserDetails
    {
        public string Title { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string HouseNumberName { get; set; } = string.Empty;
        public string? Street { get; set; }
        public string? Town { get; set; }
        public string? PostCode { get; set; }
    }


}
