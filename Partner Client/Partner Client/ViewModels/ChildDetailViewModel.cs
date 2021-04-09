using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AiForms.Dialogs;
using PartnerClient.Models;
using PartnerClient.Views;
using Plugin.CloudFirestore;
using Plugin.Media;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace PartnerClient.ViewModels
{
    public class ChildDetailViewModel : PageViewModel
    {
        private IListenerRegistration? docListener;
        private IListenerRegistration? questionnaireListener;

        private IDocumentReference? doc;
        private List<string>? alreadyUploadedImageUIDS;

        private string firstNames = string.Empty;
        private string lastNames = string.Empty;
        private Uri? profileImageURL;
        private Uri[]? imageURIs;
        private int regNumber;

        private string dateOfBirthString = string.Empty;
        private string dateOfEntryString = string.Empty;
        private string programme = string.Empty;
        private string focus = string.Empty;

        private string email = string.Empty;
        private string mobilePhone = string.Empty;
        private string homePhone = string.Empty;

        private ChildAddress address =  new ChildAddress();
        private ChildSchoolDetails school = new ChildSchoolDetails();
        private ChildHealthDetails health = new ChildHealthDetails();

        private string mamaDPI = string.Empty;
        private string papaDPI = string.Empty;

        private bool globalCareEnabled;
        private ChildGlobalCareDetails globalCare = new ChildGlobalCareDetails();
        private ObservableCollection<AdditionalChildInfoItem> additionalInfo = new ObservableCollection<AdditionalChildInfoItem>();

        private string createdDateString = string.Empty;
        private string createdBy = string.Empty;
        private string modifiedDateString = string.Empty;
        private string modifiedBy = string.Empty;

        public string FirstNames { get => firstNames; set => SetProperty(ref firstNames, value); }
        public string LastNames { get => lastNames; set => SetProperty(ref lastNames, value); }
        public Uri? ProfileImageURL { get => profileImageURL; set => SetProperty(ref profileImageURL, value, RaisePropertyChanged(nameof(HasProfileImage))); }
        public bool HasProfileImage => !(ProfileImageURL is null);
        public Uri[]? ImageURIs { get => imageURIs; set => SetProperty(ref imageURIs, value, RaisePropertyChanged(nameof(ImageCount))); }
        public int ImageCount => ImageURIs?.Length ?? 0;
        public int RegNumber { get => regNumber; set => SetProperty(ref regNumber, value); }

        public string DateOfBirthString { get => dateOfBirthString; set => SetProperty(ref dateOfBirthString, value); }
        public string DateOfEntryString { get => dateOfEntryString; set => SetProperty(ref dateOfEntryString, value); }
        public string Programme { get => programme; set => SetProperty(ref programme, value); }
        public string Focus { get => focus; set => SetProperty(ref focus, value); }

        public string Email { get => email; set => SetProperty(ref email, value); }
        public string MobilePhone { get => mobilePhone; set => SetProperty(ref mobilePhone, value); }
        public string HomePhone { get => homePhone; set => SetProperty(ref homePhone, value); }

        public ChildAddress Address { get => address; set => SetProperty(ref address, value); }
        public ChildSchoolDetails School { get => school; set => SetProperty(ref school, value); }
        public ChildHealthDetails Health { get => health; set => SetProperty(ref health, value); }

        public string MamaDPI { get => mamaDPI; set => SetProperty(ref mamaDPI, value); }
        public string PapaDPI { get => papaDPI; set => SetProperty(ref papaDPI, value); }

        public bool GlobalCareEnabled { get => globalCareEnabled; set => SetProperty(ref globalCareEnabled, value); }
        public ChildGlobalCareDetails GlobalCare { get => globalCare; set => SetProperty(ref globalCare, value); }

        public ObservableCollection<AdditionalChildInfoItem> AdditionalInfo { get => additionalInfo; set => SetProperty(ref additionalInfo, value); }

        public string CreatedDateString { get => createdDateString; set => SetProperty(ref createdDateString, value); }
        public string CreatedBy { get => createdBy; set => SetProperty(ref createdBy, value); }
        public string ModifiedDateString { get => modifiedDateString; set => SetProperty(ref modifiedDateString, value); }
        public string ModifiedBy { get => modifiedBy; set => SetProperty(ref modifiedBy, value); }

        public ChildDetailViewModel()
        {
            Title = Resources.AppResources.ChildDetails;
        }

        public ChildDetailViewModel(bool create)
        {
            if (create)
                doc = Firestore.Collection("Kids").Document();
        }

        public SortedObservableCollection<QuestionnaireCardViewModel, Questionnaire, DateTime> QuestionnaireViewModels { get; }
            = new SortedObservableCollection<QuestionnaireCardViewModel, Questionnaire, DateTime>(x => x.Source.DateCreated, true);

        public void Init(string uid)
        {
            docListener?.Remove();
            questionnaireListener?.Remove();
            doc = Firestore.Collection("Kids").Document(uid);
            docListener = doc.AddSnapshotListener(OnSnapshot);
            questionnaireListener = FirestoreCollectionService.Subscribe(doc.Collection("Questionnaires"), 
                QuestionnaireViewModels, x => new QuestionnaireCardViewModel(x));
        }

        public async Task UploadAsync(bool create)
        {
            var kid = new Kid()
            {
                FirstNames = FirstNames,
                LastNames = LastNames,
                ProfileImageUID = EditImageViewModels.SingleOrDefault(x => x.IsProfileImage)?.UID,
                RegNumber = RegNumber,
                DateOfBirth = DateOfBirthString,
                DateOfEntry = DateOfEntryString,
                Programme = Programme,
                Focus = Focus,
                Email = Email,
                MobilePhone = MobilePhone,
                HomePhone = HomePhone,
                Address = Address,
                SchoolDetails = School,
                HealthDetails = Health,
                MamaDPI = MamaDPI,
                PapaDPI = PapaDPI,
                AssignedTeamName = Team.Current!.OrganisationName,
                AssignedTeamUID = Team.Current!.UID,
                CreatedBy = create ? PartnerUser.Current!.Email : CreatedBy,
                GlobalCareEnabled = GlobalCareEnabled,
                GlobalCareDetails = GlobalCareEnabled ? GlobalCare : null,
                AdditionalInfo = AdditionalInfo.ToList(),
                ModifiedBy = PartnerUser.Current!.Email
            };
            var imagesRef = Storage.RootReference
                 .GetChild("Teams").GetChild(Team.Current!.UID)
                 .GetChild("Kids").GetChild(doc!.Id);
            if (alreadyUploadedImageUIDS?.Any() ?? false)
            {
                await Task.WhenAll(alreadyUploadedImageUIDS.Where(x => !EditImageViewModels.Any(y => y.UID == x)).Select(x => imagesRef.GetChild(x).DeleteAsync()));
                await Task.WhenAll(EditImageViewModels.Where(x => !alreadyUploadedImageUIDS.Contains(x.UID)).Select(vm => imagesRef.GetChild(vm.UID).PutStreamAsync(vm.GetStream()!)));
            }
            else
                await Task.WhenAll(EditImageViewModels.Select(vm => imagesRef.GetChild(vm.UID).PutStreamAsync(vm.GetStream()!)));
            var imageURIs = await Task.WhenAll(EditImageViewModels.Select(x => imagesRef.GetChild(x.UID).GetDownloadUrlAsync()));
            kid.Images = EditImageViewModels.Zip(imageURIs, (k, v) => new ChildImage() { Uid = k.UID, UploadedBy = k.UploadedBy, TeamUid = k.TeamUID, Uri = v.OriginalString }).ToList();
            if (create)
                await doc!.SetAsync(kid);
            else
                await doc!.UpdateAsync(kid);
        }

        private void OnSnapshot(IDocumentSnapshot? snapshot, Exception? ex)
        {
            var kid = snapshot?.ToObject<Kid>();
            EditImageViewModels.Clear();
            if (!(kid is null))
            {
                (FirstNames, LastNames) = (kid.FirstNames, kid.LastNames);
                ProfileImageURL = kid.Images.SingleOrDefault(x => x.Uid == kid.ProfileImageUID)?.Uri?.ToUri();
                ImageURIs = kid.Images.Select(x => new Uri(x.Uri)).ToArray();
                alreadyUploadedImageUIDS = kid.Images.Select(x => x.Uid).ToList();
                RegNumber = kid.RegNumber;
                DateOfBirthString = kid.DateOfBirth;
                DateOfEntryString = kid.DateOfEntry;
                (Programme, Focus) = (kid.Programme, kid.Focus);
                (Email, MobilePhone, HomePhone) = (kid.Email, kid.MobilePhone, kid.HomePhone);
                (Address, School, Health) = (kid.Address, kid.SchoolDetails, kid.HealthDetails);
                (MamaDPI, PapaDPI) = (kid.MamaDPI, kid.PapaDPI);
                (GlobalCareEnabled, GlobalCare) = (kid.GlobalCareEnabled, kid.GlobalCareDetails ?? new ChildGlobalCareDetails());
                AdditionalInfo = new ObservableCollection<AdditionalChildInfoItem>(kid.AdditionalInfo);
                (CreatedDateString, CreatedBy) = (kid.CreationTimestamp.ToShortDateString(), kid.CreatedBy);
                (ModifiedDateString, ModifiedBy) = (kid.ModificationTimestamp.ToShortDateString(), kid.ModifiedBy);
                foreach (var image in kid.Images)
                    EditImageViewModels.Add(new ChildEditImageViewModel(new Uri(image.Uri),
                        image.Uid, image.UploadedBy, image.Uid == kid.ProfileImageUID, image.TeamUid));
            }
        }

        public Command AddQuestionnaireCommand => new Command(ExecuteAddQuestionnaireCommand);
        private async void ExecuteAddQuestionnaireCommand()
            => await Shell.Current.GoToAsync($"QuestionnaireCreation?childUid={doc?.Id}");

        public ObservableCollection<ChildEditImageViewModel> EditImageViewModels { get; } = new ObservableCollection<ChildEditImageViewModel>();
        public ObservableCollection<EditableGCSponsor> EditGlobalCareSponsors { get; } = new ObservableCollection<EditableGCSponsor>();

        public Command EditCommand => new Command(ExecuteEditCommand);
        private async void ExecuteEditCommand()
        {
            docListener?.Remove();
            EditGlobalCareSponsors.Clear();
            GlobalCare.Sponsors.ForEach(x => EditGlobalCareSponsors.Add(new EditableGCSponsor(x)));
            if (await Dialog.Instance.ShowAsync<ChildEditView>(this))
            {
                GlobalCare.Sponsors = EditGlobalCareSponsors.Select(x => x.Name).ToList();
                await UploadAsync(false);
            }
            docListener = doc!.AddSnapshotListener(OnSnapshot);
        }

        public Command AddImageCommand => new Command(ExecuteAddImageCommand);

        public Command AddSponsorCommand => new Command(() => EditGlobalCareSponsors.Add(new EditableGCSponsor()));
        public Command<EditableGCSponsor> RemoveSponsorCommand => new Command<EditableGCSponsor>(x => EditGlobalCareSponsors.Remove(x));

        public Command AddInfoCommand => new Command(() => AdditionalInfo.Add(new AdditionalChildInfoItem()));
        public Command<AdditionalChildInfoItem> RemoveInfoCommand => new Command<AdditionalChildInfoItem>(x => AdditionalInfo.Remove(x));

        private async void ExecuteAddImageCommand()
        {
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync();
                if (!(file is null))
                    EditImageViewModels.Add(new ChildEditImageViewModel(file.GetStreamWithImageRotatedForExternalStorage, !EditImageViewModels.Any()));
            }
            catch { }
        }

        public Command<ChildEditImageViewModel> RemoveImageCommand => new Command<ChildEditImageViewModel>(ExecuteRemoveImageCommand);
        private void ExecuteRemoveImageCommand(ChildEditImageViewModel vm)
        {
            EditImageViewModels.Remove(vm);
            if (EditImageViewModels.Any())
                EditImageViewModels.First().IsProfileImage = true;
        }

        public Command<string> SetProfileImageCommand => new Command<string>(
                uid => EditImageViewModels.ForEach(x => x.IsProfileImage = x.UID == uid));


        public Command<QuestionnaireCardViewModel> QuestionnaireTappedCommand => new Command<QuestionnaireCardViewModel>(ExecuteQuestionnaireTappedCommand);
        private async void ExecuteQuestionnaireTappedCommand(QuestionnaireCardViewModel vm)
        {
            await Shell.Current.GoToAsync($"QuestionnaireView?childId={doc?.Id}&qId={vm.UID}&firstNames={Uri.EscapeDataString(FirstNames)}&lastNames={Uri.EscapeDataString(LastNames)}&dateString={Uri.EscapeDataString(vm.DateCreatedString)}");
        }


    }

    public class ChildEditImageViewModel : ProjectCreationImageViewModel
    {
        public string UploadedBy { get; }
        public string TeamUID { get; }

        private bool isProfileImage;
        public bool IsProfileImage { get => isProfileImage; set => SetProperty(ref isProfileImage, value); }

        public ChildEditImageViewModel(Func<Stream> getStream, bool isProfileImage) : base(getStream)
            => (UploadedBy, IsProfileImage, TeamUID) = (PartnerUser.Current!.Email, isProfileImage, Team.Current!.UID);

        public ChildEditImageViewModel(Uri uri, string uid, string uploadedBy, bool isProfileImage, string teamUID) : base(uri, uid)
            => (UploadedBy, IsProfileImage, TeamUID) = (uploadedBy, isProfileImage, teamUID);

    }

    public class EditableGCSponsor : BaseViewModel
    {

        private string name = string.Empty;
        public string Name { get => name; set => SetProperty(ref name, value); }

        public EditableGCSponsor(string name) => Name = name; 
        public EditableGCSponsor() { }
    }
}
