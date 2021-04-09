using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using SKD.Common.Models;
using SKD.Common.Themes;
using System;
using SecureStorage = Xamarin.Essentials.SecureStorage;

namespace PartnerClient.Models
{
    public class PartnerUser : BaseUser
    {
        public string? TeamUID { get; set; }
        public bool TeamConfirmed { get; set; }
        public bool IsTeamLeader { get; set; }
        public bool EmailVerificationLinkSent { get; set; }
        public bool IsAdmin { get; set; }

        //public ContactInfo ContactDetails { get; set; }

        public static event Action<PartnerUser?>? CurrentUpdated;
        public static event Action<PartnerUser?>? CurrentChanged;

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
                    CurrentChanged?.Invoke(null);
                    CurrentUpdated?.Invoke(null);
                    Team.SetCurrent(null);
                    return;
                }
                var doc = Firestore.Collection("Users").Document(uid!);
                listener = doc.AddSnapshotListener((snapshot, ex) => OnCurrentSnapshot(snapshot));
            }
        }


        private static string? previousUID;

        private static void OnCurrentSnapshot(IDocumentSnapshot? snapshot)
        {
            Current = snapshot?.ToObject<PartnerUser>();
            CurrentUpdated?.Invoke(Current);
            if(Current?.UID != previousUID)
            {
                previousUID = Current?.UID;
                CurrentChanged?.Invoke(Current);
            }
            if (Current?.TeamConfirmed ?? false)
                Team.SetCurrent(Current.TeamUID);
            if (!(Current is null) && ThemeEngine.SetTheme(Current.DesiredTheme))
                SecureStorage.SetAsync("ThemePreference", Current.DesiredTheme.ToString());
        }

        [Ignored]
        public static PartnerUser? Current { get; private set; }
    }
}
