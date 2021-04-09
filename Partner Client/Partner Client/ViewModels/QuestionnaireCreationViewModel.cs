using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using PartnerClient.Models;
using SKD.Common.Resources;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using SKD.Common.Views;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace PartnerClient.ViewModels
{
    public class QuestionnaireCreationViewModel : PageViewModel
    {
        private IDocumentReference? childDoc;
        public QuestionnaireCreationViewModel()
        {
            Title = AppResources.Questionnaire;
            IMAViewModels = Enum.GetValues(typeof(ImpactMeasurementArea))
                .OfType<ImpactMeasurementArea>().Select(x => new QuestionnaireCreationIMAViewModel(x, OnIMASelectedChanged)).ToArray();
            TemplateViewModels.CollectionChanged += (_, __) => SubmitCommand.ChangeCanExecute();
        }

        public ObservableCollection<QuestionnaireTemplateViewModel> TemplateViewModels { get; } = new ObservableCollection<QuestionnaireTemplateViewModel>();

        private async void OnIMASelectedChanged(ImpactMeasurementArea ima, bool selected)
        {
            if (selected)
                TemplateViewModels.Add(new QuestionnaireTemplateViewModel((await Firestore
                    .Collection("QuestionnaireTemplates")
                    .Document(ima.ToString()).GetAsync())
                    .ToObject<QuestionnaireTemplate>()!));
            else
                TemplateViewModels.Remove(TemplateViewModels.Single(x => x.IMA == ima));
        }

        public QuestionnaireCreationIMAViewModel[] IMAViewModels { get; }

        public void InitForChild(string uid)
        {
            childDoc = Firestore.Collection("Kids").Document(uid);
            IMAViewModels.ForEach(x => x.IsSelected = false);
        }

        private Command? _submitCommand;
        public Command SubmitCommand => _submitCommand ??= new Command(ExecuteSubmitCommand, () => TemplateViewModels.Any());
        private async void ExecuteSubmitCommand()
        {
            var questionnaire = new Questionnaire
            {
                ImpactMeasurementAreas = TemplateViewModels.Select(x => x.IMA).ToList(),
                ConductedBy = PartnerUser.Current!.Email
            };
            TemplateViewModels.SelectMany(x => x).Select(x => x.AnswerType == AnswerType.Options ? x.OptionIndex : null).Where(x => x != null).ForEach(x => System.Diagnostics.Debug.WriteLine(x));
            List<QuestionnaireAnswers> answersz = TemplateViewModels.Select(
               x => new QuestionnaireAnswers()
               {
                   ImpactMeasurementArea = x.IMA,
                   Answers = x.Where(x => x.AnswerType == AnswerType.Slider || x.OptionIndex != null)
                              .Select(x => x.AnswerType switch {
                    AnswerType.Slider => new QuestionnaireAnswer() { Question = x.Question, SliderValue = x.SliderValue },
                    AnswerType.Options => new QuestionnaireAnswer() { Question = x.Question, OptionsValue = x.Question.Options![Math.Max(x.OptionIndex ?? 0, 0)] },
                    _ => throw new InvalidEnumArgumentException("Supplied AnswerType does not exist")
                }).ToList()
               }).ToList();
            var qDoc = await childDoc!.Collection("Questionnaires").AddAsync(questionnaire);
            await Task.WhenAll(answersz.Select(x => qDoc.Collection("Answers").Document(x.ImpactMeasurementArea.ToString()).SetAsync(x)));
            await Navigation.PopAsync();
        }

    }

    public class QuestionnaireCreationIMAViewModel : CardTagViewModel
    {
        private readonly Action<ImpactMeasurementArea, bool> _onSelectedChanged;

        private bool isSelected;
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value, () => _onSelectedChanged(IMA, value)); }

        public QuestionnaireCreationIMAViewModel(ImpactMeasurementArea ima, Action<ImpactMeasurementArea, bool> onSelectedChanged) : base(ima)
            => _onSelectedChanged = onSelectedChanged;
    }

    public class QuestionnaireTemplateViewModel : List<QuestionnaireItemViewModel>

    {
        public string UID { get; }
        public ImpactMeasurementArea IMA { get; }
        public string IMAText { get; }
        public string IMAGlyph { get; }
        public Color IMAColour { get; }

        public QuestionnaireTemplateViewModel(QuestionnaireTemplate template) : base()
        {
            AddRange(template.Questions.Select(x => new QuestionnaireItemViewModel(x)));
            UID = template.UID;
            IMA = template.ImpactMeasurementArea;
            IMAText = template.ImpactMeasurementArea.GetLocalisedName();
            IMAGlyph = template.ImpactMeasurementArea.GetGlyph();
            IMAColour = template.ImpactMeasurementArea.GetAccentColour();
        }
    }

    public class QuestionnaireItemViewModel : BaseViewModel
    {
        private int? optionIndex;
        private double sliderValue;

        public double SliderValue { get => sliderValue; set => SetProperty(ref sliderValue, value); }
        public int? OptionIndex { get => optionIndex; set => SetProperty(ref optionIndex, value); }

        public QuestionnaireItem Question { get; }

        public string QuestionText { get; }
        public AnswerType AnswerType { get; }

        public FaceType? FaceType { get; }
        public List<string>? Options { get; }

        public bool ForOrganisation { get; }

        public QuestionnaireItemViewModel(QuestionnaireItem item)
        {
            Question = item;
            AnswerType = item.AnswerType;
            ForOrganisation = item.ForOrganisation;
            QuestionText = Culture.IsSpanish ? item.QuestionEs : item.QuestionEn;
            if (AnswerType == AnswerType.Slider)
            {
                FaceType = item.SliderInfo!.FaceType;
                Options = item.SliderInfo!.EffectiveTextStops.Select(x => Culture.IsSpanish ? x.Es : x.En).ToList();
            }
            else if (AnswerType == AnswerType.Options)
                Options = item.Options!.Select(x => Culture.IsSpanish ? x.Es : x.En).ToList();
        }


    }

}
