using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs;
using PartnerClient.Models;
using PartnerClient.Resources;
using PartnerClient.Services;
using PartnerClient.Views;
using Plugin.CloudFirestore;
using Plugin.Media;
using SKD.Common.Models;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using Xamarin.Forms;

namespace PartnerClient.ViewModels
{
    public class ProjectUpdateCreationViewModel : PageViewModel
    {
        private IDocumentReference? doc;
        private List<string>? alreadyUploadedImageUIDS;
        private DateTime? dateCreated;
        private bool isBusy;
        private bool isSavedDraft;

        private string? caption;
        private bool marksCompletion;

        private string? projectNameEn;
        public string? ProjectNameEn { get => projectNameEn; set => SetProperty(ref projectNameEn, value); }


        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value, SubmitCommand.ChangeCanExecute); }
        public bool IsSavedDraft { get => isSavedDraft; set => SetProperty(ref isSavedDraft, value); }

        public string? Caption { get => caption; set => SetProperty(ref caption, value, SubmitCommand.ChangeCanExecute); }
        public bool MarksCompletion { get => marksCompletion; set => SetProperty(ref marksCompletion, value); }

        public ObservableCollection<ProjectCreationImageViewModel> ImageViewModels { get; } = new ObservableCollection<ProjectCreationImageViewModel>();

        public void SetupCreateForProject(string uid)
        {
            doc = Firestore.Collection("Projects")
                .Document(uid).Collection("Updates")
                .Document();
            IsSavedDraft = false;
            DeleteDraftCommand.ChangeCanExecute();
            ImageViewModels.Clear();
            (Caption, MarksCompletion) = (string.Empty, false);
            alreadyUploadedImageUIDS = null;
            dateCreated = null;
        }

        public Command AddImageCommand => new Command(ExecuteAddImageCommand);

        public Command<ProjectCreationImageViewModel> RemoveImageCommand
            => new Command<ProjectCreationImageViewModel>(vm => ImageViewModels.Remove(vm));

        private Command<bool>? submitCommand;
        public Command<bool> SubmitCommand => submitCommand ??= new Command<bool>(ExecuteSubmitCommand, CanSubmit);

        private bool CanSubmit(bool isDraft) => !isBusy && (!string.IsNullOrWhiteSpace(Caption) || isDraft);

        private async void ExecuteAddImageCommand()
        {
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync();
                if (!(file is null))
                    ImageViewModels.Add(new ProjectCreationImageViewModel(file.GetStreamWithImageRotatedForExternalStorage));
            }
            catch { }
        }

        private async void ExecuteSubmitCommand(bool saveAsDraft)
        {
            IsBusy = true;
            bool isSpanish = Culture.IsSpanish;
            var caption = Caption ?? string.Empty;

            // Translate caption
            var translations = await TranslationService.TranslateAsync(caption);
            var translatedCaption = translations[0];

            // Create Project Update Model
            var update = new ProjectUpdate()
            {
                UID = doc!.Id,
                CaptionEn = isSpanish ? translatedCaption : caption,
                CaptionEs = isSpanish ? caption : translatedCaption,
                MarksCompletion = MarksCompletion,
                Status = saveAsDraft ? ProjectUpdateStatus.Draft : ProjectUpdateStatus.Pending,
                PartnerUID = Team.Current!.UID,
                ProjectNameEn = ProjectNameEn!,
                DatePublished = null, 
                DateCreated = dateCreated ??= DateTime.UtcNow,
                QuestionnaireTabs = selectedQuestionnaireTabViewModels?.Select(x => x.Source).ToList()
            };
            var imagesRef = Storage.RootReference
                 .GetChild("Teams").GetChild(Team.Current.UID)
                 .GetChild("Projects").GetChild(doc.Parent.Parent!.Id)
                 .GetChild("Public").GetChild("Updates")
                 .GetChild(doc.Id);
            if (alreadyUploadedImageUIDS?.Any() ?? false)
            {
                await Task.WhenAll(alreadyUploadedImageUIDS.Where(x => !ImageViewModels.Any(y => y.UID == x)).Select(x => imagesRef.GetChild(x).DeleteAsync()));
                await Task.WhenAll(ImageViewModels.Where(x => !alreadyUploadedImageUIDS.Contains(x.UID)).Select(vm => imagesRef.GetChild(vm.UID).PutStreamAsync(vm.GetStream()!)));
            }
            else
                await Task.WhenAll(ImageViewModels.Select(vm => imagesRef.GetChild(vm.UID).PutStreamAsync(vm.GetStream()!)));
            var imageUIDs = ImageViewModels.Select(x => x.UID);
            var imageURIs = await Task.WhenAll(imageUIDs.Select(x => imagesRef.GetChild(x).GetDownloadUrlAsync()));
            update.Images = imageUIDs.Zip(imageURIs, (k, v) => new ProjectImage { Uid = k, Uri = v.OriginalString }).ToList();
            await doc.SetAsync(update);
            if (saveAsDraft)
            {
                alreadyUploadedImageUIDS = update.Images.Select(x => x.Uid).ToList();
                IsSavedDraft = true;
                DeleteDraftCommand.ChangeCanExecute();
            }
            else
                await Navigation.PopAsync();
            IsBusy = false;
        }

        public async void LoadUpdateDraft(string uid, string projectUID)
        {
            bool isSpanish = Culture.IsSpanish;
            doc = Firestore.Collection("Projects")
                .Document(projectUID).Collection("Updates")
                .Document(uid);
            IsSavedDraft = true;
            DeleteDraftCommand.ChangeCanExecute();
            var update = (await doc.GetAsync()).ToObject<ProjectUpdate>()!;
            Caption = isSpanish ? update.CaptionEs : update.CaptionEn;
            MarksCompletion = update.MarksCompletion;
            dateCreated = update.DateCreated;
            SelectedQuestionnaireTabViewModels = update.QuestionnaireTabs?.Select(x => new QuestionnaireTabViewModel(x)).ToList();

            // Images
            ImageViewModels.Clear();
            alreadyUploadedImageUIDS = update.Images?.Select(x => x.Uid).ToList();
            update.Images?.ForEach(image => ImageViewModels.Add(new ProjectCreationImageViewModel(new Uri(image.Uri), image.Uid)));
        }

        private Command? _deleteDraftCommand;
        public Command DeleteDraftCommand => _deleteDraftCommand ??= new Command(ExecuteDeleteDraftCommand, () => IsSavedDraft);
        private async void ExecuteDeleteDraftCommand()
        {
            if (await Shell.Current.DisplayAlert(AppResources.DeleteDraftTitle, AppResources.DeleteDraftMessage, AppResources.Yes, AppResources.Cancel))
            {
                var imagesRef = Storage.RootReference
                     .GetChild("Teams").GetChild(Team.Current!.UID)
                     .GetChild("Projects").GetChild(doc!.Id)
                     .GetChild("Public");
                await Task.WhenAll(alreadyUploadedImageUIDS.Select(x => imagesRef.GetChild(x).DeleteAsync()));
                await doc.DeleteAsync();
                await Navigation.PopAsync();
            }
        }

        private List<QuestionnaireTabSelectionGroup>? questionnaireSelection;
        public List<QuestionnaireTabSelectionGroup>? QuestionnaireSelection { get => questionnaireSelection; private set => SetProperty(ref questionnaireSelection, value); }


        private List<QuestionnaireTabViewModel>? selectedQuestionnaireTabViewModels;
        public List<QuestionnaireTabViewModel>? SelectedQuestionnaireTabViewModels { get => selectedQuestionnaireTabViewModels; private set => SetProperty(ref selectedQuestionnaireTabViewModels, value); }


        public Command SelectQuestionnairesCommand => new Command(ExecuteSelectQuestionnairesCommand);
        private async void ExecuteSelectQuestionnairesCommand()
        {
            var kidsSnapshot = await Firestore.Collection("Kids").WhereEqualsTo(nameof(Kid.AssignedTeamUID), Team.Current!.UID).GetAsync();
            var kids = kidsSnapshot.ToObjects<Kid>();
            var questionnairesByKid = (await Task.WhenAll(kidsSnapshot.Documents.Select(x => x.Reference.Collection("Questionnaires").GetAsync())))
                .Select(x => x.ToObjects<Questionnaire>()
                .OrderByDescending(x => x.DateCreated));
            QuestionnaireSelection = kids.Zip(questionnairesByKid, (kid, qS) => new QuestionnaireTabSelectionGroup(kid, qS.Select(x => new QuestionnaireTabSelectionViewModel(GetQuestionnaireTab(kid, x))))).ToList();
            if (await Dialog.Instance.ShowAsync<QuestionnaireSelectionView>(this))
            {
                SelectedQuestionnaireTabViewModels = QuestionnaireSelection.SelectMany(x => x)
                    .Where(x => x.IsSelected)
                    .Select(x => (QuestionnaireTabViewModel)x)
                    .OrderByDescending(x => x.Source.DateCreated).ToList();
                if (SelectedQuestionnaireTabViewModels.Count == 0)
                    SelectedQuestionnaireTabViewModels = null;
            }
        }

        private QuestionnaireTab GetQuestionnaireTab(Kid kid, Questionnaire q) => new QuestionnaireTab()
        {
            ChildFirstNames = kid.FirstNames,
            ChildLastNames = kid.LastNames,
            DateCreated = q.DateCreated,
            ChildUID = kid.UID,
            QuestionnaireUID = q.UID,
            ImpactMeasurementAreas = q.ImpactMeasurementAreas,
        };
    }

    public class QuestionnaireTabSelectionGroup : List<QuestionnaireTabSelectionViewModel>
    {
        public string ChildUID { get; }
        public string ChildFirstNames { get; }
        public string ChildLastNames { get; }

        public QuestionnaireTabSelectionGroup(Kid kid, IEnumerable<QuestionnaireTabSelectionViewModel> items) : base(items) {
            ChildUID = kid.UID;
            ChildFirstNames = kid.FirstNames;
            ChildLastNames = kid.LastNames;
        } 
    }

    public class QuestionnaireTabSelectionViewModel : QuestionnaireTabViewModel
    {

        private bool isSelected;
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

        public QuestionnaireTabSelectionViewModel(QuestionnaireTab tab) : base(tab) { }
    }

}
