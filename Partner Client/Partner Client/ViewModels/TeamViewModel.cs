using System.Collections;
using System.Collections.ObjectModel;
using PartnerClient.Models;
using PartnerClient.Resources;
using Plugin.CloudFirestore;
using SKD.Common.Services;
using SKD.Common.ViewModels;
using Xamarin.Forms;
using Functions = PartnerClient.Services.CloudFunctions;

namespace PartnerClient.ViewModels
{
    public class TeamViewModel : PageViewModel
    {


        private TeamLeader leader = new TeamLeader();
        public TeamLeader Leader { get => leader; set => SetProperty(ref leader, value); }

        public TeamMemberViewModelCollection<TeamMemberViewModel> Members { get; } = new TeamMemberViewModelCollection<TeamMemberViewModel>(AppResources.Members);
        public TeamMemberViewModelCollection<TeamMemberRequestViewModel> MemberRequests { get; } = new TeamMemberViewModelCollection<TeamMemberRequestViewModel>(AppResources.MemberRequests);

        public ObservableCollection<IEnumerable> MembersAndRequests { get; }

        private string authCode = string.Empty;
        public string AuthCode { get => authCode; private set => SetProperty(ref authCode, value); }



        private bool isTeamLeader;
        public bool IsTeamLeader { get => isTeamLeader; set => SetProperty(ref isTeamLeader, value); }

        private IListenerRegistration? memberListener;
        private IListenerRegistration? memberRequestListener;
        public TeamViewModel()
        {
            MembersAndRequests = new ObservableCollection<IEnumerable> { Members };
            wasTeamLeader = PartnerUser.Current?.IsTeamLeader ?? false;
            var team = Team.Current;
            OnTeamUpdated(team);
            OnUserChanged(PartnerUser.Current);
            AuthCode = team?.AuthCode ?? string.Empty;
            if (!(team is null))
            {
                memberListener = FirestoreCollectionService
                    .Subscribe<TeamMember, TeamMemberViewModel>(team.Doc.Collection("Members"), Members, x => new TeamMemberViewModel(x));
                if (IsTeamLeader)
                {
                    MembersAndRequests.Add(MemberRequests);
                    memberRequestListener = FirestoreCollectionService
                        .Subscribe<TeamMember, TeamMemberRequestViewModel>(team.Doc.Collection("MemberRequests"),
                        MemberRequests, x => new TeamMemberRequestViewModel(x));
                }
            }
            Team.CurrentUpdated += OnTeamUpdated;
            PartnerUser.CurrentUpdated += OnUserChanged;
            Team.CurrentChanged += t =>
            {
                AuthCode = t?.AuthCode ?? string.Empty;
                memberListener?.Remove();
                memberRequestListener?.Remove();
                if (t is null)
                    return;
                memberListener = FirestoreCollectionService
                    .Subscribe<TeamMember, TeamMemberViewModel>(t.Doc.Collection("Members"), Members, x => new TeamMemberViewModel(x));
                if (IsTeamLeader)
                    memberRequestListener = FirestoreCollectionService
                        .Subscribe<TeamMember, TeamMemberRequestViewModel>(t.Doc.Collection("MemberRequests"), 
                        MemberRequests, x => new TeamMemberRequestViewModel(x));
            };
        }

        private bool wasTeamLeader;
        private void OnUserChanged(PartnerUser? user)
        {
            IsTeamLeader = user?.IsTeamLeader ?? false;
            LeaveTeamCommand.ChangeCanExecute();
            if (IsTeamLeader && !wasTeamLeader && memberRequestListener is null)
            {
                MembersAndRequests.Add(MemberRequests);
                memberRequestListener = FirestoreCollectionService
                    .Subscribe<TeamMember, TeamMemberRequestViewModel>(Team.Current!.Doc.Collection("MemberRequests"), 
                    MemberRequests, x => new TeamMemberRequestViewModel(x));
            }
            else if (!IsTeamLeader && wasTeamLeader && MembersAndRequests.Contains(MemberRequests))
                MembersAndRequests.Remove(MemberRequests);
            wasTeamLeader = IsTeamLeader;
        }

        private void OnTeamUpdated(Team? team)
        {
            if (team is null)
                return;

            Title = team.OrganisationName;
            Leader = team.Leader;
        }

        // Don't need to set a 'CanExecute' for these 3 because they will not even be visible if the user isn't teh team leader;
        public Command<string> AcceptJoinRequestCommand => new Command<string>(async uid
            => await Functions.HandleJoinTeamRequestAsync(true, uid));
        public Command<string> RejectJoinRequestCommand => new Command<string>(async uid
            => await Functions.HandleJoinTeamRequestAsync(false, uid));

        public Command<TeamMember> TransferOwnershipCommand => new Command<TeamMember>(ExecuteTransferOwnershipCommand);
        private async void ExecuteTransferOwnershipCommand(TeamMember member)
        {
            if (await Shell.Current.DisplayAlert(AppResources.TransferOwnershipTitle,
                string.Format(AppResources.TransferOwnershipMessage, member.Name),
                AppResources.Okay,
                AppResources.Cancel))
                await Functions.TransferTeamOwnershipAsync(member.UID);
        }

        private Command? _leaveTeamCommand;
        public Command LeaveTeamCommand => _leaveTeamCommand ??= new Command(async () => await Functions.LeaveTeamAsync(), () => !IsTeamLeader);



        public Command SettingsCommand => new Command(ExecuteSettingsCommand);
        private async void ExecuteSettingsCommand()
        {
            await Shell.Current.GoToAsync("Settings");
        }

    }

    public class TeamMemberViewModelCollection<T> : ObservableCollection<T> where T : ITeamMemberViewModel
    {
        public string Name { get; }
        public TeamMemberViewModelCollection(string name) : base() => Name = name;
    }

    public interface ITeamMemberViewModel
    {
        public string Name { get; }
        public string Email { get; }
    }

    public class TeamMemberViewModel : CardViewModel<TeamMember>, ITeamMemberViewModel
    {

        private string name;
        public string Name { get => name; private set => SetProperty(ref name, value); }

        private string email;
        public string Email { get => email; private set => SetProperty(ref email, value); }

        public TeamMemberViewModel(TeamMember member) : base(member)
         => (name, email) = (member.Name, member.Email);
        

        public override void Update(TeamMember member)
        {
            base.Update(member);
            (Name, Email) = (member.Name, member.Email);
        }

    }

    public class TeamMemberRequestViewModel : CardViewModel<TeamMember>, ITeamMemberViewModel
    {

        private string name;
        public string Name { get => name; private set => SetProperty(ref name, value); }

        private string email;
        public string Email { get => email; private set => SetProperty(ref email, value); }

        public TeamMemberRequestViewModel(TeamMember member) : base(member)
            => (name, email) = (member.Name, member.Email);


        public override void Update(TeamMember member)
        {
            base.Update(member);
            (Name, Email) = (member.Name, member.Email);
        }

    }

}
