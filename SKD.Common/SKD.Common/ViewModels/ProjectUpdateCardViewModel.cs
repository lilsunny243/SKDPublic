using System;
using System.Collections.Generic;
using System.Linq;
using SKD.Common.Models;
using SKD.Common.Utils;

namespace SKD.Common.ViewModels
{
    public class ProjectUpdateCardViewModel : IndexedCardViewModel<ProjectUpdate>
    {
        private string caption;
        private string? dateString;
        private Uri[]? imageURIs;
        private bool marksCompletion;
        private ProjectUpdateStatus status = ProjectUpdateStatus.Draft;

        private List<QuestionnaireTabViewModel>? questionnaireTabViewModels;
        public List<QuestionnaireTabViewModel>? QuestionnaireTabViewModels { get => questionnaireTabViewModels; private set => SetProperty(ref questionnaireTabViewModels, value); }



        public string ProjectNameEn { get; private set; }
        public string Caption { get => caption; private set => SetProperty(ref caption, value); }
        public string? DateString { get => dateString; private set => SetProperty(ref dateString, value); }
        public ProjectUpdateStatus Status { get => status; private set => SetProperty(ref status, value); }
        public Uri[]? ImageURIs { get => imageURIs; private set => SetProperty(ref imageURIs, value, RaisePropertyChanged(nameof(ImageCount))); }
        public int ImageCount => ImageURIs?.Length ?? 0;
        public bool MarksCompletion { get => marksCompletion; private set => SetProperty(ref marksCompletion, value); }
        public bool IsDraft => Status == ProjectUpdateStatus.Draft;
        public bool IsPending => Status == ProjectUpdateStatus.Pending;
        public bool IsPublished => Status == ProjectUpdateStatus.Published;
        public bool PublishedMarksCompletion => IsPublished && MarksCompletion;
        public bool AnyQuestionnaires => QuestionnaireTabViewModels?.Any() ?? false;

        public ProjectUpdateCardViewModel(ProjectUpdate update) : base(update)
        {
            ProjectNameEn = update.ProjectNameEn;
            caption = Culture.IsSpanish ? update.CaptionEs : update.CaptionEn;
            DateString = update.DatePublished?.ToShortDateString();
            ImageURIs = update.Images?.Select(x => new Uri(x.Uri)).ToArray();
            MarksCompletion = update.MarksCompletion;
            Status = update.Status;
            QuestionnaireTabViewModels = update.QuestionnaireTabs?.Select(x => new QuestionnaireTabViewModel(x)).ToList();
        }

        public override void Update(ProjectUpdate update)
        {
            base.Update(update);
            Caption = Culture.IsSpanish ? update.CaptionEs : update.CaptionEn;
            DateString = update.DatePublished?.ToShortDateString();
            ImageURIs = update.Images?.Select(x => new Uri(x.Uri)).ToArray();
            MarksCompletion = update.MarksCompletion;
            Status = update.Status;
            QuestionnaireTabViewModels = update.QuestionnaireTabs?.Select(x => new QuestionnaireTabViewModel(x)).ToList();
            OnPropertyChanged(nameof(IsDraft));
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(IsPublished));
            OnPropertyChanged(nameof(PublishedMarksCompletion));
            OnPropertyChanged(nameof(AnyQuestionnaires));
        }
    }
}
