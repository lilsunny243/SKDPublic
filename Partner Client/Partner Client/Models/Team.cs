using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using Plugin.CloudFirestore.Converters;
using SKD.Common.Models;

namespace PartnerClient.Models
{
    public class Team : BaseModel
    {
        private static IFirestore Firestore => CrossCloudFirestore.Current.Instance;

        [Ignored]
        public IDocumentReference Doc => Firestore.Collection("Teams").Document(UID);

        public string OrganisationName { get; set; } = string.Empty;
        public TeamLeader Leader { get; set; } = new TeamLeader();

        [DocumentConverter(typeof(EnumStringConverter))]
        public TeamStatus Status { get; set; }

        public ContactInfo ContactDetails { get; set; } = new ContactInfo();
        public string AuthCode { get; set; } = string.Empty;

        public static event Action<Team?>? CurrentChanged;
        public static event Action<Team?>? CurrentUpdated;

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
                var doc = Firestore.Collection("Teams").Document(uid!);
                listener = doc.AddSnapshotListener((snapshot, ex) => OnCurrentSnapshot(snapshot));
            }
        }

        private static string? previousUID;
        private static void OnCurrentSnapshot(IDocumentSnapshot? snapshot)
        {
            Current = snapshot?.ToObject<Team>();
            CurrentUpdated?.Invoke(Current);
            if(previousUID != Current?.UID)
            {
                previousUID = Current?.UID;
                CurrentChanged?.Invoke(Current);
            }
        }

        [Ignored]
        public static Team? Current { get; private set; }
    }

    public class TeamLeader
    {
        public string UID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class TeamMember : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ContactInfo
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public enum TeamStatus
    {
       Active,
       Retired
    }

    public class TeamCreationRequest : BaseModel
    {
        public string OrganisationName { get; set; } = string.Empty;
        public ContactInfo ContactDetails { get; set; } = new ContactInfo();
        public TeamLeader Leader { get; set; } = new TeamLeader();
        public string AuthCode { get; set; } = string.Empty;
        public List<string> AreasOfWork { get; set; } = new List<string>();
        public List<string> AvailableDocuments { get; set; } = new List<string>();

    }

    public class TeamJoinResponse
    {
        public TeamJoinResponse(bool accepted, string memberUid)
            => (Accepted, MemberUID) = (accepted, memberUid);

        public bool Accepted { get; }
        public string MemberUID { get; }
    }

    public class TeamOwnershipTransfer
    {
        public TeamOwnershipTransfer(string newLeaderUID) => NewLeaderUID = newLeaderUID;

        public string NewLeaderUID { get; }
    }

}
