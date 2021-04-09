using System;
using System.Collections.Generic;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using Plugin.CloudFirestore.Converters;
using SKD.Common.Utils;

namespace SKD.Common.Models
{
    public class Project : BaseModel
    {
        public string NameEn { get; set; } = string.Empty;
        public string NameEs { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionEs { get; set; } = string.Empty;
        public string PartnerUID { get; set; } = string.Empty;
        public string PartnerName { get; set; } = string.Empty;

        public bool IsUrgent { get; set; }

        [DocumentConverter(typeof(EnumStringConverter))]
        public ProjectNotifyAllStatus NotifyAllStatus { get; set; } = ProjectNotifyAllStatus.Disabled;

        public List<ProjectImage> Images { get; set; } = new List<ProjectImage>();

        // Key: Impact Measurement Area, Value: Qualification/Explanation/What/How        
        [DocumentConverter(typeof(ProjectIMADictionaryConverter))]
        public Dictionary<ImpactMeasurementArea, ImpactMeasurementAreaExplanation> ImpactMeasurementAreas { get; set; } = new Dictionary<ImpactMeasurementArea, ImpactMeasurementAreaExplanation>();

        [DocumentConverter(typeof(EnumStringConverter))]
        public ImpactMeasurementArea PrimaryImpactMeasurementArea { get; set; }

        [DocumentConverter(typeof(EnumStringConverter))]
        public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

        public int Raised { get; set; }
        public int Target { get; set; }

        public List<CostBreakdownComponent> CostBreakdown { get; set; } = new List<CostBreakdownComponent>();

        public DateTime? DatePublished { get; set; } = null;

        public List<string>? DonorUIDs { get; set; }
    }

    public class ImpactMeasurementAreaExplanation
    {
        public string En { get; set; } = string.Empty;
        public string Es { get; set; } = string.Empty;
    }

    public class ProjectImage
    {
        public string Uid { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
    }

    public enum ProjectStatus
    {
        Draft,
        Pending,
        Active,
        Completed
    }

    public enum ProjectNotifyAllStatus
    {
        Disabled,
        NotSent,
        Sent
    }

    public class CostBreakdownSubcomponent
    {
        public string NameEn { get; set; } = string.Empty;
        public string NameEs { get; set; } = string.Empty;
        public int UnitCost { get; set; }
        public int Quantity { get; set; }
    }

    public class CostBreakdownComponent : CostBreakdownSubcomponent
    {
        public List<CostBreakdownSubcomponent>? Children { get; set; }
    }

    public enum ProjectUpdateStatus
    {
        Draft,
        Pending,
        Published
    }

    public class ProjectUpdate : BaseModel
    {
        public string PartnerUID { get; set; } = string.Empty;
        public string ProjectNameEn { get; set; } = string.Empty;

        [DocumentConverter(typeof(EnumStringConverter))]
        public ProjectUpdateStatus Status { get; set; } = ProjectUpdateStatus.Draft;

        public string CaptionEn { get; set; } = string.Empty;
        public string CaptionEs { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }

        public DateTime? DatePublished { get; set; }
        public bool MarksCompletion { get; set; }
        public List<ProjectImage>? Images { get; set; }
        public List<QuestionnaireTab>? QuestionnaireTabs { get; set; }
        public List<string>? AllowedDonorUIDs { get; set; }
    }

}
