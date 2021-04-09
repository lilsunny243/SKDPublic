using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKD.Common.Models;
using Xamarin.Forms;

namespace SKD.Common.ViewModels
{
    public class QuestionnaireTabViewModel : CardViewModel<QuestionnaireTab>
    {
        private readonly string childId;
        private readonly string qId;

        private string childFirstNames;
        private string childLastNames;
        private string dateCreatedString;
        private List<CardTagViewModel> tagViewModels;

        public string ChildFirstNames { get => childFirstNames; private set => SetProperty(ref childFirstNames, value); }
        public string ChildLastNames { get => childLastNames; private set => SetProperty(ref childLastNames, value); }
        public string DateCreatedString { get => dateCreatedString; private set => SetProperty(ref dateCreatedString, value); }
        public List<CardTagViewModel> TagViewModels { get => tagViewModels; private set => SetProperty(ref tagViewModels, value); }

        public QuestionnaireTabViewModel(QuestionnaireTab tab) : base(tab)
        {
            childId = tab.ChildUID;
            qId = tab.QuestionnaireUID;
            (childFirstNames, childLastNames) = (tab.ChildFirstNames, tab.ChildLastNames);
            dateCreatedString = tab.DateCreated.ToShortDateString();
            tagViewModels = tab.ImpactMeasurementAreas.Select(x => new CardTagViewModel(x)).ToList();
        }

        public override void Update(QuestionnaireTab tab)
        {
            base.Update(tab);
            (ChildFirstNames, ChildLastNames) = (tab.ChildFirstNames, tab.ChildLastNames);
            DateCreatedString = tab.DateCreated.ToShortDateString();
            TagViewModels = tab.ImpactMeasurementAreas.Select(x => new CardTagViewModel(x)).ToList();
        }

        public Command TappedCommand => new Command(ExecuteTappedCommand);
        private async void ExecuteTappedCommand()
        {
            await Shell.Current.GoToAsync($"QuestionnaireView?childId={childId}&qId={qId}&firstNames={Uri.EscapeDataString(ChildFirstNames)}&lastNames={Uri.EscapeDataString(ChildLastNames)}&dateString={Uri.EscapeDataString(DateCreatedString)}");
        }

    }
    public class QuestionnaireCardViewModel : IndexedCardViewModel<Questionnaire>
    {
        private string dateCreatedString;
        private List<CardTagViewModel> tagViewModels;

        public string DateCreatedString { get => dateCreatedString; private set => SetProperty(ref dateCreatedString, value); }
        public List<CardTagViewModel> TagViewModels { get => tagViewModels; private set => SetProperty(ref tagViewModels, value); }

        public QuestionnaireCardViewModel(Questionnaire questionnaire) : base(questionnaire)
        {
            dateCreatedString = questionnaire.DateCreated.ToShortDateString();
            tagViewModels = questionnaire.ImpactMeasurementAreas.Select(x => new CardTagViewModel(x)).ToList();
        }

        public override void Update(Questionnaire questionnaire)
        {
            base.Update(questionnaire);
            DateCreatedString = questionnaire.DateCreated.ToShortDateString();
            TagViewModels = questionnaire.ImpactMeasurementAreas.Select(x => new CardTagViewModel(x)).ToList();
        }
    }
}
