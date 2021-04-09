using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using Plugin.CloudFirestore.Converters;
using SKD.Common.Utils;
using SKD.Common.Views;
using System.Linq;

namespace SKD.Common.Models
{
    public class Questionnaire : BaseModel
    {
        [ServerTimestamp(CanReplace = false)]
        public DateTime DateCreated { get; set; }

        public string ConductedBy { get; set; } = string.Empty;

        [DocumentConverter(typeof(EnumListConverter<ImpactMeasurementArea>))]
        public List<ImpactMeasurementArea> ImpactMeasurementAreas { get; set; } = new List<ImpactMeasurementArea>();
    }

    public class QuestionnaireAnswers : BaseModel
    {
        [DocumentConverter(typeof(EnumStringConverter))]
        public ImpactMeasurementArea ImpactMeasurementArea { get; set; }

        public List<QuestionnaireAnswer> Answers { get; set; } = new List<QuestionnaireAnswer>();
    }

    public class QuestionnaireTemplate : BaseModel
    {
        [DocumentConverter(typeof(EnumStringConverter))]
        public ImpactMeasurementArea ImpactMeasurementArea { get; set; }

        public List<QuestionnaireItem> Questions { get; set; } = new List<QuestionnaireItem>();
    }

    public class QuestionnaireAnswer
    {
        public double? SliderValue { get; set; }
        public QuestionAnswerOption? OptionsValue { get; set; }
        public QuestionnaireItem Question { get; set; } = new QuestionnaireItem();
    }

    public class QuestionnaireItem
    {
        public string QuestionEn { get; set; } = string.Empty;
        public string QuestionEs { get; set; } = string.Empty;

        [DocumentConverter(typeof(EnumStringConverter))]
        public AnswerType AnswerType { get; set; }

        public QuestionSliderInfo? SliderInfo { get; set; }
        public List<QuestionAnswerOption>? Options { get; set; }

        public bool ForOrganisation { get; set; }
    }

    public class QuestionSliderInfo
    {

        [DocumentConverter(typeof(EnumStringConverter))]
        public FaceType FaceType { get; set; }

        private readonly List<QuestionAnswerOption> _textStops = new List<QuestionAnswerOption>() {
            new QuestionAnswerOption(){ En = defaultNegativeEn, Es = defaultNegativeEs },
            new QuestionAnswerOption(){ En = defaultNeutralMinusEn, Es = defaultNeutralMinusEs },
            new QuestionAnswerOption(){ En = defaultNeutralPlusEn, Es = defaultNeutralPlusEs },
            new QuestionAnswerOption(){ En = defaultPositiveEn, Es = defaultPositiveEs },
        };

        public List<QuestionAnswerOption>? TextStops { get; set; }

        [Ignored]
        public List<QuestionAnswerOption> EffectiveTextStops => ReverseTextStops ? Rev(TextStops ?? _textStops) : TextStops ?? _textStops;

        private List<T> Rev<T>(IEnumerable<T> items) => items.Reverse().ToList();

        public bool ReverseTextStops { get; set; }

        private const string defaultNegativeEn = "Not at all\n/ Rarely";
        private const string defaultNeutralMinusEn = "Somewhat\n/ Sometimes";
        private const string defaultNeutralPlusEn = "Very\n/ Often";
        private const string defaultPositiveEn = "Extremely\n/ Almost Always";
        private const string defaultNegativeEs = "Nunca\n/ Rara vez";
        private const string defaultNeutralMinusEs = "Un poco\n/ A veces";
        private const string defaultNeutralPlusEs = "Mucho\n/ Muchas veces";
        private const string defaultPositiveEs = "Extremadamente\n/ Casi siempre";
    }

    public class QuestionAnswerOption
    {
        public string En { get; set; } = string.Empty;
        public string Es { get; set; } = string.Empty;
    }

    public class QuestionnaireTab : BaseModel
    {
        public string ChildFirstNames { get; set; } = string.Empty;
        public string ChildLastNames { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }

        public string ChildUID { get; set; } = string.Empty;
        public string QuestionnaireUID { get; set; } = string.Empty;


        [DocumentConverter(typeof(EnumListConverter<ImpactMeasurementArea>))]
        public List<ImpactMeasurementArea> ImpactMeasurementAreas { get; set; } = new List<ImpactMeasurementArea>();
    }

    public enum AnswerType
    {
        Slider,
        Options,
    }
}
