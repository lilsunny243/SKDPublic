using System;
using System.Collections.Generic;
using System.Text;
using Plugin.CloudFirestore.Attributes;
using SKD.Common.Models;

namespace PartnerClient.Models
{
    public class Kid : BaseModel
    {
        [ServerTimestamp(CanReplace = false)]
        public DateTime CreationTimestamp { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        [ServerTimestamp(CanReplace = true)]
        public DateTime ModificationTimestamp { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;

        public int RegNumber { get; set; }
        public string Focus { get; set; } = string.Empty;
        public string Programme { get; set; } = string.Empty;

        public string FirstNames { get; set; } = string.Empty;
        public string LastNames { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string DateOfEntry { get; set; } = string.Empty;

        public string? ProfileImageUID { get; set; }
        public List<ChildImage> Images { get; set; } = new List<ChildImage>();

        public string? AssignedTeamName { get; set; }
        public string? AssignedTeamUID { get; set; }

        public ChildAddress Address { get; set; } = new ChildAddress();
        public string Email { get; set; } = string.Empty;
        public string MobilePhone { get; set; } = string.Empty;
        public string HomePhone { get; set; } = string.Empty;

        public ChildSchoolDetails SchoolDetails { get; set; } = new ChildSchoolDetails();

        public string MamaDPI { get; set; } = string.Empty;
        public string PapaDPI { get; set; } = string.Empty;

        public bool GlobalCareEnabled { get; set; }
        public ChildGlobalCareDetails? GlobalCareDetails { get; set; }

        public ChildHealthDetails HealthDetails { get; set; } = new ChildHealthDetails();

        public List<AdditionalChildInfoItem> AdditionalInfo { get; set; } = new List<AdditionalChildInfoItem>();
    }

    public class ChildAddress
    {
        public string Line1 { get; set; } = string.Empty;
        public string Line2 { get; set; } = string.Empty;
        public string Line3 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }

    public class ChildSchoolDetails
    {
        public string Name { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string OpeningTimes { get; set; } = string.Empty;
    }

    public class ChildGlobalCareDetails
    {
        public string Reference { get; set; } = string.Empty;
        public List<string> Sponsors { get; set; } = new List<string>();
    }

    public class ChildHealthDetails
    {
        public string DoctorClinicName { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
    }

    public class AdditionalChildInfoItem
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ChildImage
    {
        public string Uid { get; set; } = string.Empty;
        public string TeamUid { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
    }
}
