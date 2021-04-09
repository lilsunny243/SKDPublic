using AiForms.Dialogs;
using PartnerClient.Models;
using PartnerClient.Services;
using PartnerClient.Views;
using Plugin.CloudFirestore;
using SKD.Common.Services;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using System;
using System.Linq;
using System.Net;
using Xamarin.Forms;

namespace PartnerClient.ViewModels
{
    public class KidsViewModel : PageViewModel
    {
        private IListenerRegistration? listener;
        private string? selectedChildUID;
        
        public string? SelectedChildUID { get => selectedChildUID; private set => SetProperty(ref selectedChildUID, value); }

        public KidsViewModel()
        {
            Title = Resources.AppResources.Kids;
            Team.CurrentChanged += OnTeamChanged;
            OnTeamChanged(Team.Current);
        }

        public SortedObservableCollection<ChildCardViewModel, Kid, int> KidViewModels { get; } = new SortedObservableCollection<ChildCardViewModel, Kid, int>(x => x.Source.RegNumber);

        private void OnTeamChanged(Team? team)
        {
            listener?.Remove();
            if(!(team is null))
            {
                var query = Firestore.Collection("Kids").WhereEqualsTo(nameof(Kid.AssignedTeamUID), team.UID);
                listener = FirestoreCollectionService.Subscribe(query, KidViewModels, x => new ChildCardViewModel(x));
            }
        }

        public Command<Kid> CardTappedCommand => new Command<Kid>(ExecuteCardTappedCommand);
        private async void ExecuteCardTappedCommand(Kid kid)
        {
            SelectedChildUID = kid.UID;
            await Shell.Current.GoToAsync($"ChildDetails?uid={kid.UID}");
        }

        public Command AddChildCommand => new Command(ExecuteAddChildCommand);
        private async void ExecuteAddChildCommand()
        {
            var vm = new ChildDetailViewModel(true);
            if (await Dialog.Instance.ShowAsync<ChildEditView>(vm))
            {
                vm.RegNumber = await CloudFunctions.GetNextChildNumber();
                await vm.UploadAsync(true);
            }
        }


    }

    public class ChildCardViewModel : IndexedCardViewModel<Kid>
    {
        private string firstNames;
        private string lastNames;
        private Uri? imageURL;
        private int regNumber;
        private bool globalCareEnabled;

        public string FirstNames { get => firstNames; private set => SetProperty(ref firstNames, value); }
        public string LastNames { get => lastNames; private set => SetProperty(ref lastNames, value); }
        public Uri? ImageURL { get => imageURL; private set => SetProperty(ref imageURL, value, RaisePropertyChanged(nameof(HasProfileImage))); }
        public bool HasProfileImage => !(ImageURL is null);
        public int RegNumber { get => regNumber; private set => SetProperty(ref regNumber, value); }
        public bool GlobalCareEnabled { get => globalCareEnabled; private set => SetProperty(ref globalCareEnabled, value); }

        
        public Command AddQuestionnaireCommand => new Command(ExecuteAddQuestionnaireCommand);
        private async void ExecuteAddQuestionnaireCommand()
            => await Shell.Current.GoToAsync($"QuestionnaireCreation?childUid={UID}");

        public ChildCardViewModel(Kid kid) : base(kid)
        {
            (firstNames, lastNames) = (kid.FirstNames, kid.LastNames);
            imageURL = kid.Images.SingleOrDefault(x => x.Uid == kid.ProfileImageUID)?.Uri?.ToUri();
            regNumber = kid.RegNumber;
            globalCareEnabled = kid.GlobalCareEnabled;
        }

        public override void Update(Kid kid)
        {
            base.Update(kid);
            (FirstNames, LastNames) = (kid.FirstNames, kid.LastNames);
            ImageURL = kid.Images.SingleOrDefault(x => x.Uid == kid.ProfileImageUID)?.Uri.ToUri();
            RegNumber = kid.RegNumber;
            GlobalCareEnabled = kid.GlobalCareEnabled;
        }
    }

}
