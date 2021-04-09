using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;
using System.Text;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Utils;
using SKD.Common.Views;
using SKD.Common.Resources;

namespace SKD.Common.ViewModels
{

    public class QuestionnaireViewModel : PageViewModel
    {

        private List<QuestionnaireAnswerGroup>? answerGroups;
        public List<QuestionnaireAnswerGroup>? AnswerGroups { get => answerGroups; private set => SetProperty(ref answerGroups, value); }


        private string childFirstNames = string.Empty;
        private string childLastNames = string.Empty;
        private string dateString = string.Empty;

        public string ChildFirstNames { get => childFirstNames; set => SetProperty(ref childFirstNames, value); }
        public string ChildLastNames { get => childLastNames; set => SetProperty(ref childLastNames, value); }
        public string DateString { get => dateString; set => SetProperty(ref dateString, value); }


        public QuestionnaireViewModel()
        {
            Title = AppResources.Questionnaire;
        }

        public async void Init(string childId, string qId)
        {
            var answers = (await Firestore
                .Collection("Kids").Document(childId)
                .Collection("Questionnaires").Document(qId)
                .Collection("Answers").GetAsync())
            .ToObjects<QuestionnaireAnswers>().ToList();
            AnswerGroups = answers.Select(x => new QuestionnaireAnswerGroup(x.ImpactMeasurementArea, x.Answers)).ToList();
        }
    }

    public class QuestionnaireAnswerGroup : List<QuestionnaireAnswerViewModel>
    {
        public ImpactMeasurementArea IMA { get; }
        public string IMAText { get; }
        public Color IMAColour { get; }
        public string IMAGlyph { get; }

        public QuestionnaireAnswerGroup(ImpactMeasurementArea ima, IEnumerable<QuestionnaireAnswer> answers) : base(answers.Select(x => new QuestionnaireAnswerViewModel(x)))
        {
            IMA = ima;
            IMAText = ima.GetLocalisedName();
            IMAColour = ima.GetAccentColour();
            IMAGlyph = ima.GetGlyph();
        }

    }

    public class QuestionnaireAnswerViewModel
    {
        public string QuestionText { get; }
        public AnswerType AnswerType { get; }
        public double? SliderValue { get; }
        public string? OptionText { get; }

        public FaceType? FaceType { get; }
        public List<string>? Options { get; }

        public bool ForOrganisation { get; }

        public QuestionnaireAnswerViewModel(QuestionnaireAnswer answer)
        {
            QuestionText = Culture.IsSpanish ? answer.Question.QuestionEs : answer.Question.QuestionEn;
            AnswerType = answer.Question.AnswerType;
            SliderValue = answer.SliderValue;
            OptionText = Culture.IsSpanish ? answer.OptionsValue?.Es : answer.OptionsValue?.En;
            ForOrganisation = answer.Question.ForOrganisation;
            if (AnswerType == AnswerType.Slider)
            {
                FaceType = answer.Question.SliderInfo!.FaceType;
                Options = answer.Question.SliderInfo!.EffectiveTextStops.Select(x => Culture.IsSpanish ? x.Es : x.En).ToList();
            }
            else if (AnswerType == AnswerType.Options)
                Options = answer.Question.Options!.Select(x => Culture.IsSpanish ? x.Es : x.En).ToList();
        }
    }
}
